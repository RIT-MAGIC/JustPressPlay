var value;
$(document).ready(function () {
    $("#scanInfo").hide();
    if($('#IsRepeatable').val('true')){
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