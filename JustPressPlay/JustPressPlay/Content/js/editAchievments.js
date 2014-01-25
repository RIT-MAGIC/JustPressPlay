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
});