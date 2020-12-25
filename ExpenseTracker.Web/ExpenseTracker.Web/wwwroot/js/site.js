// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function onToggleElement(elId) {
    var el = document.getElementById(elId);
    if (el.style.display == "none") {
        el.style.display = "revert"
    } else {
        el.style.display = "none"
    }
}

document.querySelectorAll(".auto-select").forEach(e => {
    e.onfocus = ev => { ev.srcElement.select();}
})