using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "btn-del")]
    public class DeleteButtonTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var childContent = await output.GetChildContentAsync();
            output.Attributes.Add("class", "btn btn-danger rounded-circle");
            output.Attributes.Add("style", "width:30px;height:30px");
            output.Content.SetHtmlContent("X");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}