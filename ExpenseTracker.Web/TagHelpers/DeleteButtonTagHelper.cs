using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ExpenseTracker.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "btn-del")]
    public class DeleteButtonTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("class", "btn");
            output.Attributes.Add("style", "color: red; font-size: 23px; vertical-aling: middle; text-align:center;");
            output.Content.SetHtmlContent("&#10008;");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}