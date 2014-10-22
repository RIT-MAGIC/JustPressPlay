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