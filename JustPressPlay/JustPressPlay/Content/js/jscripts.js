/* 
 * jscripts.js
 * Just Press Play v3
 */

// Creates and injects the remove friend button
// @param playerID ID of the player to remove
// @param targetID ID of the page element target location
function createRemoveFriendForm(playerID, targetID) {

    // Ensure we have a target location to append to
    if (targetID == null) {
        console.log("Friend Forms must have a targetID");
        return;
    }

    var tID = document.getElementById(targetID);
    $(tID).empty();

    // Build the form
    var form = document.createElement("form");
    $(form).addClass("remove-friend-form").attr("action", "/Players/RemoveFriend").attr("method", "post").append(
        "<input type=\"hidden\" name=\"id\" value=\"" + playerID + "\" /><button type=\"submit\" class=\"expand flip-text remove-friend\"><span class=\"text-1\">You are friends</span><span class=\"text-2\">Remove Friend</span></button>"
    );

    // Bind submission to submit with ajax
    $(form).submit(function (e) {
        e.preventDefault();

        $(form).ajaxSubmit({
            clearForm: false,
            success: function (responseObj) {
                createAddFriendForm(playerID, targetID);
            },
            error: function () {
                alert("Friend removal failed. Please try again later.");
            }
        });
    });

    // Add form to target
    $(tID).append(form);
}

// Creates and injects the add friend button
// @param playerID ID of the player to add
// @param targetID ID of the page element target location
function createAddFriendForm(playerID, targetID) {

    if (targetID == null) {
        console.log("Friend Forms must have a targetID");
        return;
    }

    var tID = document.getElementById(targetID);
    $(tID).empty();

    var form = document.createElement("form");
    $(form).addClass("add-friend-form").attr("action", "/Players/AddFriend").attr("method", "post").append(
        "<input type=\"hidden\" name=\"id\" value=\"" + playerID + "\" /><button type=\"submit\" class=\"expand add-friend\">Add Friend</button>"
    );

    $(form).submit(function (e) {
        e.preventDefault();

        $(form).ajaxSubmit({
            clearForm: false,
            success: function (responseObj) {
                createPendingFriendButton(targetID);
            },
            error: function () {
                alert("Friend addition failed. Please try again later.");
            }
        });
    });

    // Add form to target
    $(tID).append(form);
}

// Creates and injects the pending friend response forms
// @param playerID ID of the player to update
// @param targetID ID of the page element target location
// @param inline Bool to flip display mode to inline. Used for notification view.
function createPendingFriendResponseForm(playerID, targetID, inline) {

    if (targetID == null) {
        console.log("Friend Forms must have a targetID");
        return;
    }

    var tID = document.getElementById(targetID);
    //$(tID).empty();

    var form1 = document.createElement("form");
    $(form1).addClass("accept-friend-form").attr("action", "/Players/AcceptFriendRequest").attr("method", "post").append(
        "<input type=\"hidden\" name=\"id\" value=\"" + playerID + "\" /><input type=\"submit\" value=\"Accept\" class=\"button save " + (inline ? "inline-block" : "expand") + "\" />"
    );
    if (inline) $(form1).addClass("inline-block");

    var form2 = document.createElement("form");
    $(form2).addClass("ignore-friend-form").attr("action", "/Players/IgnoreFriendRequest").attr("method", "post").append(
        "<input type=\"hidden\" name=\"id\" value=\"" + playerID + "\" /><input type=\"submit\" value=\"Ignore\" class=\"button cancel " + (inline ? "inline-block" : "expand") + "\" />"
    );
    if (inline) $(form2).addClass("inline-block");

    var form3 = document.createElement("form");
    $(form3).addClass("decline-friend-form").attr("action", "/Players/DeclineFriendRequest").attr("method", "post").append(
        "<input type=\"hidden\" name=\"id\" value=\"" + playerID + "\" /><input type=\"submit\" value=\"Decline\" class=\"button cancel " + (inline ? "inline-block" : "expand") + "\" />"
    );
    if (inline) $(form3).addClass("inline-block");



    $(form1).submit(function (e) {
        e.preventDefault();

        $(form1).ajaxSubmit({
            clearForm: false,
            success: function (responseObj) {
                if (inline) {
                    $(tID).remove();
                }
                else {
                    createRemoveFriendForm(playerID, targetID);
                }
            },
            error: function () {
                alert("Friend response failed. Please try again later.");
            }
        });
    });

    $(form2).submit(function (e) {
        e.preventDefault();

        $(form2).ajaxSubmit({
            clearForm: false,
            success: function (responseObj) {
                if (inline) {
                    $(tID).remove();
                }
                else {
                    createAddFriendForm(playerID, targetID);
                }
            },
            error: function () {
                alert("Friend response failed. Please try again later.");
            }
        });
    });

    $(form3).submit(function (e) {
        e.preventDefault();

        $(form3).ajaxSubmit({
            clearForm: false,
            success: function (responseObj) {

                if (inline) {
                    $(tID).remove();
                }
                else {
                    createAddFriendForm(playerID, targetID);
                }
            },
            error: function () {
                alert("Friend response failed. Please try again later.");
            }
        });
    });

    // Add forms to target
    $(tID).append(form1);
    $(tID).append(form2);
    $(tID).append(form3);
}

// Creates and injects the pending friend button
// @param targetID ID of the page element target location
function createPendingFriendButton(targetID) {

    if (targetID == null) {
        console.log("Friend Forms must have a targetID");
        return;
    }

    var tID = document.getElementById(targetID);
    $(tID).empty();

    var button = document.createElement("button");
    $(button).addClass("expand disabled").append("Friend Request Sent");

    // Add button to target
    $(tID).append(button);
}



$(document).ready(function () {

    /***************************
        SITE-WIDE FEATURES
    ***************************/

    FastClick.attach(document.body);


    // NAVIGATION DROPDOWNS

    var activeDropdownId = "";

    $('body').click(function (e) {

        if ($(e.target).closest('.dropdownContainer').length > 0)
        {   // Click on dropdown
            //e.stopPropigation();
            return;
        }


        if ($(e.target).closest('[data-dropdownid]').length > 0)
        {   // Click on dropdown button
            var parentButton = $(e.target).closest('[data-dropdownid]');
            var targetId = parentButton.data('dropdownid');

            e.preventDefault();

            $('#' + targetId).toggleClass('active');

            if (targetId == activeDropdownId)
                activeDropdownId = "";
            else
            {
                $('#' + activeDropdownId).toggleClass('active', false);
                activeDropdownId = targetId;
            }


            return;
        }
           
        $('#' + activeDropdownId).toggleClass('active', false);
        activeDropdownId = "";

    });

    // END NAVIGATION DROPDOWNS



    // SCROLL BOXES
    
    $('.scroll').slimScroll();

    $('.dropdownScroll').slimScroll({
        height: '250px',
        size: '6px',
        position: 'right',
        color: '#000',
        alwaysVisible: false,
        distance: '1px',
        railVisible: false,
        railDraggable: true,
        railColor: '#222',
        railOpacity: 0.3,
        wheelStep: 10,
        allowPageScroll: false,
        disableFadeOut: false
    });




    
    
});

