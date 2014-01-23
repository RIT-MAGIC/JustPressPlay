var numReqs = 1; // Init - we start with one required achievement
$("#rmv-btn").hide(); //Hide the remove achievement button by default

//Called from the addAchievement button
function addRequirement() {
    $("#rmv-btn").show(); 
    if (numReqs < 6) {
        numReqs++;
        $(".AchievementReq").append("<div class='row collapse' id='req" + numReqs + "'><div class='small-3 large-2 columns'><span class='prefix'>Requirement " + numReqs + "</span></div><div class='small-9 large-10 columns achievmentRequirments'><input id='RequirementsList' type='text' value='' name='RequirementsList'><span class='field-validation-valid' data-valmsg-replace='true' data-valmsg-for='RequirementsList'></span></div></div>");
        if (numReqs == 6) {
            $("#add-btn").hide();
        }
    }
}

function removeRequirement() {
    $("#add-btn").show();
    if (numReqs > 1) {
        $("#req" + numReqs).remove();
        numReqs--;
        if (numReqs == 1) {
            $("#rmv-btn").hide();
        }
    }
}