var value;
$(document).ready(function () {
    $("#repeatDelay").hide();
    value = $("#Type option:selected").text();
    value = value.toLowerCase();
});

$('#IsRepeatable').click(function () {
    $("#repeatDelay").toggle(this.checked);
});

$("#Type").change(function () {
    var selection = parseInt($("#Type").val());

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
});