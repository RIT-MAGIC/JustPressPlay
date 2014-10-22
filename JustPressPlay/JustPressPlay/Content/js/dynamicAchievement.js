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

var value;
$(document).ready(function () {
    $("#scanInfo").hide();
    if($('#IsRepeatable').is(':checked')){
        $("#repeatDelay").show();
    }else{
        $("#repeatDelay").hide();
    }
    value = $("#Type option:selected").text();
    value = value.toLowerCase();
    updateForm();
});

$('#IsRepeatable').click(function () {
    $("#repeatDelay").toggle(this.checked);
});


function updateForm() {
    var selection = parseInt($("#Type option:selected").val());

    switch (selection) {
        case 0:
            $("#" + value + "Info").css("display", "none");
            $("#scanInfo").css("display", "inline");
            value = $("#Type option:selected").text();
            value = value.toLowerCase();
            break;
        case 1:
            $("#" + value + "Info").css("display", "none");
            $("#systemInfo").css("display", "inline");
            value = $("#Type option:selected").text();
            value = value.toLowerCase();
            break;
        case 2:
            $("#" + value + "Info").css("display", "none");
            $("#thresholdInfo").css("display", "inline");
            value = $("#Type option:selected").text();
            value = value.toLowerCase();
            break;
        case 3:
            $("#" + value + "Info").css("display", "none");
            $("#usersubmissionInfo").css("display", "inline");
            value = $("#Type option:selected").text();
            value = value.toLowerCase();
            break;
        case 4:
            $("#" + value + "Info").css("display", "none");
            $("#adminassignedInfo").css("display", "inline");
            value = $("#Type option:selected").text();
            value = value.toLowerCase();
            break;
        default:
            $("#" + value + "Info").css("display", "none");
            $("#scanInfo").css("display", "inline");
            value = $("#Type option:selected").text();
            value = value.toLowerCase();
            break;
    }
}

$("#Type").change(function () {
    updateForm();
});