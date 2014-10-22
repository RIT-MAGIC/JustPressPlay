/*
 * Copyright 2014 Rochester Institute of Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
    jQuery("#modal-content").append("<div id='main-content'><p>Are you sure you want to discard your draft?</p><input type='button' value='Yes' onclick='discard()'/><input type='button' value='No' onclick='hideModal()'/></div>");
}


function checkDiscard() {
    jQuery("#modal-content, #modal-background").addClass("active");
}

function discard() {
    var form = jQuery('#discardForm');

    form.submit();
}

function hideModal() {
    jQuery("#modal-content, #modal-background").removeClass("active");
}