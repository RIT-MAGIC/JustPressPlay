﻿/* 
 * jscripts.js
 * Just Press Play
 *
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
    var activeDropdownButton = null;
    
    var updateArrowPosition = function () {
        // Update position of navigation dropdown arrow on page resize.
        if (activeDropdownButton != null) {
            var width = activeDropdownButton.parent().width();
            var left = activeDropdownButton.parent().position().left;
            var center = left + (width / 2);
            $('#navigation-dropdown-arrow').css('left', center);
        }
    }

    var exitCurrentDropdown = function () {
        $('#' + activeDropdownId).toggleClass('active', false);
        $('[data-dropdownid="' + activeDropdownId + '"]').toggleClass('active', false);
        activeDropdownId = "";
    }

    var exitEveryDropdownElement = function () {
        exitCurrentDropdown();
        $('#navigation-dropdown-arrow').toggleClass('active', false);
        activeDropdownButton = null;
    }

    $(window).resize(function () {
        updateArrowPosition();
    });

    $('body').click(function (e) {

        // Click on dropdown, do nothing
        if ($(e.target).closest('.dropdown-menu').length > 0 || $(e.target).closest('.up-arrow').length > 0 || $(e.target).closest('#nav-menu').length > 0)
        {
            return;
        }

        // Click on dropdown button
        if ($(e.target).closest('[data-dropdownid]').length > 0)
        {
            activeDropdownButton = $(e.target).closest('[data-dropdownid]');
            var targetId = activeDropdownButton.data('dropdownid');

            e.preventDefault();

            $('#' + targetId).toggleClass('active');
            $('#navigation-dropdown-arrow').toggleClass('active');
            if (targetId == 'nav-menu') {
                $('#navigation-dropdown-arrow').toggleClass('dark', true);
            }
            else {
                $('#navigation-dropdown-arrow').toggleClass('dark', false);
            }

            // Exit dropdown
            if (targetId == activeDropdownId) {
                exitCurrentDropdown();
            }
            else {
                exitCurrentDropdown();

                $('#navigation-dropdown-arrow').toggleClass('active', true);
                activeDropdownId = targetId;

                // Activate button
                $('[data-dropdownid="' + activeDropdownId + '"]').toggleClass('active', true);

                updateArrowPosition();
            }

            return;
        }

        // Click anywhere else, exit dropdown
        exitEveryDropdownElement();
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

