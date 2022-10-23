// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function toggleElement(elId) {
    var el = document.getElementById(elId);
    if (el.style.display == "none") {
        el.style.display = "revert"
    } else {
        el.style.display = "none"
    }

    return el.style.display;
}

document.querySelectorAll(".auto-select").forEach(e => {
    e.onfocus = ev => { ev.srcElement.select(); }
})

function postAjax(address, onSuccess, onFail, data) {
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            if (this.status == 200) {
                onSuccess(this);
            } else {
                onFail(this);
            }
        }
    };

    xhttp.open("POST", address, true);
    xhttp.setRequestHeader("Content-type", "application/json");
    xhttp.setRequestHeader("XSRF-TOKEN", document.querySelector('input[name="__RequestVerificationToken"]').value);
    if (data) {
        data = JSON.stringify(data);
    } else {
        data = '';
    }

    xhttp.send(data);
}