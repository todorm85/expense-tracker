using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ExpenseTracker.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "btn-apply")]
    public class ApplyButtonTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("class", "btn");
            output.Attributes.Add("style", "width: 30px; height:30px; padding:3px; margin-right: 15px; font-size: 20px;");
            output.Content.SetHtmlContent("&#10004;");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}