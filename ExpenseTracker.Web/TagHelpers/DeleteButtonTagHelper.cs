using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ExpenseTracker.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "btn-del")]
    public class DeleteButtonTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("class", "btn");
            output.Attributes.Add("style", "width:30px;height:30px;color: red; padding: 3px; font-size: 23px;");
            output.Content.SetHtmlContent("&#10008;");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}