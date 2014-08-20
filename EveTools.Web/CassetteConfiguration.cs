using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using Cassette;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.TinyIoC;
using Cassette.Utilities;

namespace EveTools.Web
{
    /// <summary>
    /// Configures the Cassette asset bundles for the web application.
    /// </summary>
    public class CassetteBundleConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<ScriptBundle>("Scripts",
            new[] {
                      "jquery-2.1.1.js",
                      "bootstrap.js",
                      "angular.js",
                      "angular-route.js",
                      "angular-animate.js",
                      "i18n/angular-locale_en-us.js",
                },
                b => b.NotSorted());

            bundles.Add<StylesheetBundle>("css",
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-theme.css",
                      "~/Scripts/angular-csp.css",
                      "~/Content/site.css");

            bundles.AddPerSubDirectory<ScriptBundle>("Scripts/app", b => b.AddReference("~/Scripts"));

            bundles.AddPerSubDirectory<HtmlTemplateBundle>("partials");
        }
    }

    public static class BundleExtensions
    {
        public static T NotSorted<T>(this T bundle) where T : Bundle
        {
            bundle.GetType().GetProperty("IsSorted", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bundle, false);
            return bundle;
        }
    }


    /// <summary>
    /// Configure Cassette to 'do the right thing' for HTML templates.
    /// Fixes the identifiers used, the content type, and the way JS template-loaders are generated.
    /// 
    /// Useful examples from http://www.walkercoderanger.com/blog/2014/03/
    /// </summary>
    public class SetupAngularHtmlTemplateConfiguration : IConfiguration<TinyIoCContainer>, IBundlePipelineModifier<HtmlTemplateBundle>
    {
        /// <summary>
        /// Use the right identifier-generation for Angular
        /// </summary>
        public void Configure(TinyIoCContainer container)
        {
            // keep the filename + extension as the identifier for html templates
            container.Register<IHtmlTemplateIdStrategy>(new HtmlTemplateIdBuilder(true, true));
        }

        /// <summary>
        /// Modify the default html template bundle pipeline to work with Angular (debug + release modes)
        /// </summary>
        public IBundlePipeline<HtmlTemplateBundle> Modify(IBundlePipeline<HtmlTemplateBundle> pipeline)
        {
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                // separate <script type="text/ng-template/> tags
                var index = pipeline.IndexOf<ParseHtmlTemplateReferences>();
                pipeline.Insert(index, new AssignContentType("text/ng-template"));
            }
            else
            {
                // merge html templates into a single javascript file, which registers the templates via $templateCache
                pipeline.ReplaceWith<JavaScriptHtmlTemplatePipeline>();
                var index = pipeline.IndexOf<WrapJavaScriptHtmlTemplates>();
                pipeline.RemoveAt(index);
                pipeline.Insert(index, new AngularWrapJavaScriptHtmlTemplates());
            }

            return pipeline;
        }

        /// <summary>
        /// Derivation of <see cref="Cassette.HtmlTemplates.WrapJavaScriptHtmlTemplates"/> to use <see cref="AngularWrapJavaScriptHtmlTemplatesTransformer"/>.
        /// </summary>
        public class AngularWrapJavaScriptHtmlTemplates : IBundleProcessor<HtmlTemplateBundle>
        {
            public void Process(HtmlTemplateBundle bundle)
            {
                if (bundle.Assets.Count == 0)
                    return;
                if (bundle.Assets.Count > 1)
                    throw new ArgumentException("AngularWrapJavaScriptHtmlTemplates should only process a bundle where the assets have been concatenated.", "bundle");
                bundle.Assets[0].AddAssetTransformer(new AngularWrapJavaScriptHtmlTemplatesTransformer());
            }
        }

        /// <summary>
        /// Derivation of <see cref="Cassette.HtmlTemplates.WrapJavaScriptHtmlTemplatesTransformer"/> to output the right wrapping JS to load the templates into angular's cache
        /// </summary>
        public class AngularWrapJavaScriptHtmlTemplatesTransformer : IAssetTransformer
        {
            public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
            {
                return (Func<Stream>)(() => (Stream)new MemoryStream(Encoding.UTF8.GetBytes(string.Format((string)"(function() {{\r\nangular.module('app').run(['$templateCache',function($templateCache) {{\r\nvar addTemplate = function(id, content) {{\r\n    $templateCache.put(id, content);\r\n}};\r\n{0}\r\n}}]);}})());", StreamExtensions.ReadToEnd(openSourceStream())))));
            }
        }
    }

}