/*
 * JPP Timeline - Jquery plugin
 * Handles the creation and update of a Timeline Feed
 * Built by Brandon Littell for Just Press Play (http://play.rit.edu/)
 * 2013
 * Probably on a MIT license
 */


// TODO: On update, set new element at which to load next earnings on scroll
// TODO: Minifiy!

(function ($) {

    $.fn.jpptimeline = function (options) {

        var self = this;
        var currentUserID = null;
        var selectorClass = '.playerSelector';
        var earningCount = 0;                           // The current number of earnings displayed
        var ajaxData = null;
        var scrollBuffer = 200;

        // Create settings object
        var settings = $.extend({
            userID: null,               // ID 
            achievementID: null,
            questID: null,
            baseURL: null,
            startIndex: 0,
            loadInterval: 10,
            dateOrder: 0
        }, options);


        // INIT Function


        /* FUNCTIONS */

        // Sets timeline variables back to initial state and clears containing div
        var reset = function () {
            currentUserID = null;
            earningCount = 0;
            ajaxData = null;

            clear();
        };

        // Clears containing div
        var clear = function () {
            self.html('');
        }

        // Reloads timeline for a new userID and other setting variables
        // PARAM: userID - Number id for a user
        $.fn.jpptimeline.loadUser = function (userID) {

            // Don't load if we are already displaying the user's data
            if (userID !== currentUserID) {

                reset();

                settings.userID = currentUserID = userID;
                //currentUserID = userID;

                $.fn.jpptimeline.load();
            }
        };
        

        // Loads earnings based upon settings object, appends to parent div
        $.fn.jpptimeline.load = function () {

            settings.startIndex = earningCount;

            $.ajax({
                url: buildAjaxURL(),
                dataType: "json",
                type: "POST",
                success: function (data) {
                
                    ajaxData = data;

                    buildEarnings();

                    $("#loading").hide();

                    // Clear loaded data
                    delete ajaxData;

                },
                error: function (e) {

                    self.append('<div class="timelinePost">' +
                                    '<h5>Whoops! Something went wrong</h5>' +
                                '</div>');

                    return false;
                }
            });

            // Don't place anything here; ajax is async and we don't know when it will finish
        };

        $.fn.jpptimeline.loadNext = function () {

        };



        /*#region Build Functions*/

        // Builds the URL to be queried by Ajax
        // RETURNS: URL String
        var buildAjaxURL = function() {
            var query = settings.baseURL + '/JSON/earnings?';

            if(settings.questID != null)
                query += 'questID=' + settings.questID + '&';

            if(settings.achievementID != null)
                query += 'achievementID=' + settings.achievementID + '&';

            if(settings.userID != null)
                query += 'id=' + settings.userID + '&';

            if(settings.startIndex != null && settings.startIndex >= 0)
                query += 'start=' + settings.startIndex + '&';

            if(settings.loadInterval != null && settings.loadInterval >= 0)
                query += 'count=' + settings.loadInterval + '&';

            return query;
        };

        
        // Builds every possible earning for loadInterval or count, whichever has a lower value
        var buildEarnings = function() {

            var earnings = ajaxData.Earnings;

            // For the number of earnings to build
            for ( var i = 0; i < ( settings.loadInterval < earnings.length ? settings.loadInterval : earnings.length); i++ ) {

                // Build each
                $(self).append(buildEarning(earnings[i]));

                ++earningCount;

            }
            if (earnings.length <= 0) {
                //$(self).append('<h5>This user hasn\'t earned this item yet</h5>');
            }
            else {
                console.log("bind scroll");
                $(window).bind('scroll', bindScroll);
            }
        };


        // Builds a single earning
        // PARAM: Specific earning data object
        // RETURN: jQuery element
        var buildEarning = function (earningData) {

            // Create post container
            $earningDiv = $(document.createElement('div')).addClass('timelinePost');

            // Insert Photo
            if (earningData.StoryPhoto != null) {
                $earningDiv.append('<div class="postPhoto"><img src="' + settings.baseURL + 'earningData.StoryPhoto" /></div>');
            }

            // Insert Link


            // Insert post info
            $earningDiv.append(buildPostInfo(earningData));

            // Insert post body
            $earningDiv.append(buildPostBody(earningData));


            return $earningDiv;

        }


        // Builds earning information header
        // PARAM: Specific earning data object
        // RETURN: jQuery element
        var buildPostInfo = function (data) {
            $postInfoDiv = $(document.createElement('div')).addClass('postInfo');
            if (data.PlayerImage == null) {
                $postInfoDiv.append('<div class="userPhoto">' +
                                        '<a href="' + settings.baseURL + '/Players/' + data.PlayerID + '">' +
                                            '<img src="' + settings.baseURL + '/Content/Images/Jpp/defaultProfileAvatar.png" />' +
                                        '</a>' +
                                    '</div>');
            }
            else {

            }
            $postInfoDiv.append('<h1>' +
                                    '<a href="' + settings.baseURL + '/Players/' + data.PlayerID + '">' +
                                        data.DisplayName +
                                    '</a>' +
                                '</h1>');
            $postInfoDiv.append('<h2>' +
                                    'Earned <a href="' + settings.baseURL + '/Achievements/' + data.Achievement.ID + '">' + data.Achievement.Title + '</a>' +
                                '</h2>');
            $postInfoDiv.append('<h3>' +
                                    data.EarnedDate +
                                '</h3>');

            return $postInfoDiv;
        };


        // Builds earning body including comments
        // PARAM: Specific earning data object
        // RETURN: jQuery element
        var buildPostBody = function (data) {
            $postBodyDiv = $(document.createElement('div')).addClass('postBody');

        
            var story = (data.StoryText == null) ? data.DisplayName + ' hasen\'t added a story yet.' : data.StoryText;
            $postBodyDiv.append('<p>' + story + '</p>');

            $postBodyDiv.append('<hr />');

            

            $postBodyDiv.append('<div class="commentsContainer">' +
                                    '<h5>' + data.Comments.Comments.length + ' Comments</h5>' +

                                    '<div class="writeComment">' +
                                        '<div class="userPhoto hide-for-small">' +
                                            '<img src="' + settings.baseURL + '/Content/Images/Jpp/defaultProfileAvatar.png" />' +
                                        '</div>' +
                                        '<div class="commentInput">' +
                                            '<input type="text" placeholder="Write a comment..." />' +
                                        '</div>' +
                                    '</div>' +
                                '</div>');


            /*
            <div class="postBody">
                                        
               
    
                <hr />
     
                <div class="commentsContainer">
                    <h5>0 Comments</h5>
    
                        <hr />
                                                    
                        <div class="writeComment">
                            <div class="userPhoto hide-for-small">
    
                                    <img src="Url.Content(ViewBag.userImage)" />
    
                                    <img src="Url.Content("~/Content/Images/Jpp/defaultProfileAvatar.png")" />
    
                            </div>
                            <div class="commentInput">
                                <input type="text" id="Text1" placeholder="Write a comment..." />
                            </div>
                        </div>
                    }
                </div>
            </div>
            */

            return $postBodyDiv;
        }
        /*#endregion*/




        var bindScroll = function () {
            if ($(window).scrollTop() + $(window).height() >= $(self)[0].scrollHeight + $(self).offset().top - scrollBuffer) {
                $(window).unbind('scroll');
                console.log("LOADING MOAR");
                $.fn.jpptimeline.load();
            }
        };




        currentUserID = settings.userID;
        //$(self).
        $.fn.jpptimeline.load();


        $("#loading").ajaxStart(function () {
            $("#loading").show();
        });

        $("#loading").ajaxStop(function () {
            
        });


        return this;

    };

}(jQuery));
