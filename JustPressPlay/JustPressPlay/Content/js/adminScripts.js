var numReqs = 1; // Init - we start with one required achievement
$("#rmv-btn").hide(); //Hide the remove achievement button by default

//Called from the addAchievement button
function addRequirement() {
    $("#rmv-btn").show();
    if (numReqs < 6) {
        numReqs++;
        var currentReq = "#req"+numReqs;
        $(currentReq).css("display", "inline");
        
        if (numReqs === 6) {
            $("#add-btn").hide();
        }
    }
}

function removeRequirement() {
    $("#add-btn").show();
    if (numReqs > 1) {
        var currentReq = "#req" + numReqs;
        var currentField = currentReq + " .small-9 input";
        $(currentField).val('');
        $(currentReq).css("display", "none");
        numReqs--;

        if (numReqs === 1) {
            $("#rmv-btn").hide();
        }
    }
}