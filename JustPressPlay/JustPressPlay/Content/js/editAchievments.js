$(document).ready(function () {
    for (var i = 1; i < 7; i++) {
        var currentReq = "#req" + i;
        var currentField = currentReq + " .small-9 input";
        currentField = $(currentField).val();
        if (currentField !== '') {
            $(currentReq).css("display", "inline");
            if (i !== 1) {
                $("#rmv-btn").show();
            }
        } else {
            numReqs = i - 1;
            break;
        }
    }

    if ($("#State").val() == 0) {
        $("#disregard").show();
    } else {
        $("#disregard").hide();
    }

    buildModal();
});

function buildModal() {
    var overlay = jQuery('<div id="modal-background"></div><div id="modal-content"></div>');
    overlay.appendTo(document.body);
    jQuery("#modal-content").append("<div id='main-content'><p>Are you sure you want to discard your draft?</p><input type='button' value='Yes' onclick='passID()'/><input type='button' value='No' onclick='hideModal()'/></div>");
}


function checkDiscard() {
    jQuery("#modal-content, #modal-background").addClass("active");
}

function hideModal() {
    jQuery("#modal-content, #modal-background").removeClass("active");
}

function passID() {
    var pathName = window.location.pathname.split('/');
    var id = pathName.pop;
    console.log(id);
}