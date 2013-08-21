/*
 * JPP Timeline - Jquery plugin
 * Handles the creation and update of a Timeline Feed
 * Built by Brandon Littell for Just Press Play (http://play.rit.edu/)
 * 2013
 * Probably on a MIT license
 */


// TODO: Minifiy!

(function ($) {

    $.fn.jpptimeline = function (options) {

        var self = $(this);
        var currentUserID = null;
        var selectorClass = '.playerSelector';
        var timelineClass = 'timelineFeed';
        var $bottom;
        var $spinner;
        var $feed;
        var earningCount = 0;                           // The current number of earnings displayed
        var ajaxData = null;
        var scrollBuffer = 200;

        // Create settings object
        var settings = $.extend({
            userID: null,                   // userID is the logged-in user visiting this page
            achievementID: null,
            questID: null,
            baseURL: null,
            startIndex: 0,
            loadInterval: 5,
            dynamicUser: false
        }, options);



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
            $feed.html('');
        }

        // Determines if a value n is a number
        // PARAM: n - unitless value to check
        // RETURNS: boolean true/false
        // Found at: http://stackoverflow.com/questions/18082/validate-numbers-in-javascript-isnumeric/1830844#1830844
        var isNumber = function (n) {
            return !isNaN(parseFloat(n)) && isFinite(n);
        }

        // Reloads timeline for a new userID and other setting variables
        // PARAM: userID - Number id for a user
        $.fn.jpptimeline.loadUser = function (userID) {

            console.log('loadUser called');

            // Don't load if we are already displaying the user's data
            if (userID !== currentUserID) {

                reset();

                $spinner.show();

                currentUserID = userID;

                $.fn.jpptimeline.load();
            }
        };
        

        // Loads earnings based upon settings object and currentUserID, appends to parent div
        $.fn.jpptimeline.load = function () {

            //console.log('load Called');

            // Load where we left off
            settings.startIndex = earningCount;

            // Make sure the user id is a number
            if (!isNumber(currentUserID)) {
                currentUserID = null;
            }

            // Only show the loading spinner if there is probably more stuff to load
            if (earningCount % settings.loadInterval == 0)
                $spinner.show();

            $.ajax({
                url: buildAjaxURL(),
                dataType: "json",
                type: "POST",
                success: function (data) {

                    ajaxData = data;

                    // If we should display general stuffs & submission form
                    if (settings.dynamicUser && settings.userID != null)
                    {

                    }

                    buildEarnings();

                    // Hide loading div after earnings have been appended
                    $spinner.hide();

                    // Only set the hash if needed
                    if (settings.dynamicUser && settings.userID != null) location.hash = currentUserID;

                    // Clear loaded data
                    delete ajaxData;

                },
                error: function (e) {

                    $spinner.hide();

                    // Error message
                    $feed.append('<div style="text-align: center; margin-bottom: 0.9375em;">' +
                                    '<span class="radius alert label expand">Whoops! Something went wrong! As part of our apology try clicking <a href="#" onclick="cornify_add();return false;">this link</a><script type="text/javascript" src="http://www.cornify.com/js/cornify.js"></script>.</span>' +
                                '</div>');

                    return false;
                }
            });

            // Don't place anything here; ajax calls are async and we don't know when they finish
        };



        /*#region Build Functions*/

        // Builds the URL to be queried by Ajax
        // RETURNS: URL String
        var buildAjaxURL = function() {
            var query = settings.baseURL + '/JSON/Earnings?';

            if(settings.questID != null)
                query += 'questID=' + settings.questID + '&';

            if(settings.achievementID != null)
                query += 'achievementID=' + settings.achievementID + '&';


            if (currentUserID != null)
            {
                query += 'id=' + currentUserID + '&';
            }
            else if (settings.userID != null)
            {
                query += 'id=' + settings.userID + '&';
            }

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

                // Check for a change in user
                /*
                if (settings.dynamicUser && settings.userID != earnings[i].playerID)
                {
                    $.fn.jpptimeline.loadUser(settings.userID);
                    return;
                }
                */

                // Build each
                //console.log('Adding earning');
                $feed.append(buildEarning(earnings[i]));
                //console.log('Earning added');

                ++earningCount;
                //console.log('Earning count: ' + earningCount + '\n');

            }
            if (earnings.length > 0) {
                //$(self).append('<div class="timelinePost"><p>This user hasn\'t earned this item yet<p></div>');
                console.log("bind scroll");
                $(window).bind('scroll', bindScroll);
            }
            if (earnings.length == 0 && earningCount <= 0) {
                $feed.append('<div class="timelinePost"><div class="postInfo"><h1>This user hasn\'t earned this item yet<h1></div></div>');
                ++earningCount;
            }
        };


        // Builds a single earning
        // PARAM: Specific earning data object
        // RETURN: jQuery element
        var buildEarning = function (earningData) {

            // Create post container
            $earningDiv = $(document.createElement('div')).addClass('timelinePost');

            // Insert Photo
            if (earningData.EarningIsAchievement && earningData.StoryPhoto != null) {
                $earningDiv.append('<div class="postPhoto"><img src="' + settings.baseURL + '/' + earningData.StoryPhoto + '" /></div>');
            }

            // Insert Link


            // Insert post info
            //console.log('Build post info');
            $earningDiv.append(buildPostInfo(earningData));

            // Insert post body
            //console.log('Build post body');
            if (earningData.EarningIsAchievement)
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
                                    'Earned <a href="' + settings.baseURL + '/Achievements/' + data.TemplateID + '">' + data.Title + '</a>' +
                                '</h2>');
            var date = new Date(parseInt(data.EarnedDate.substr(6))).toLocaleDateString();
            $postInfoDiv.append('<h3>' +
                                    date +
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
                                    '<h5>' + data.Comments.length + ' Comments</h5>' +

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





        // Callback for scroll event to handle additional loading
        var bindScroll = function () {
            // Check for a scroll to the bottom of the timelineFeed - scrollBuffer
            if ($(window).scrollTop() + $(window).height() >= self[0].scrollHeight + self.offset().top - scrollBuffer) {
                $(window).unbind('scroll');
                $.fn.jpptimeline.load();
            }
        };



        // INIT
        var init = function () {

            // Look for a hashed id on load
            if (settings.dynamicUser && location.hash.length > 1) {
                console.log('init hash: ' + location.hash.substr(1));
                currentUserID = location.hash.substr(1);
            }

            // Create feed container
            var $tempFeed = $(document.createElement('div')).addClass('timelineFeed');
            self.append($tempFeed);
            $feed = self.children('.timelineFeed');

            // Add bottom
            var $bot = $(document.createElement('div')).addClass('bottom');
            $bot.append('<div class="spinner"></div>');
            $bot.append('<div class="endOfFeed"><h6>End of Feed</h6><div>');
            self.append($bot);
            $bottom = self.children('.bottom');
            $spinner = $bottom.children('.spinner');

            console.log('Init userID: ' + currentUserID);

            // Initial load
            $.fn.jpptimeline.load();
        }

        init();
        

        //window.onhashchange = $.fn.jpptimeline.loadUser(location.hash.substring(1));




        $(window).on('hashchange', function () {

            var hash = location.hash.substring(1);

            if (settings.dynamicUser && currentUserID != hash) {
                console.log('hash change: ' + hash);
                $.fn.jpptimeline.loadUser(hash);
            }

        });


        $spinner.ajaxStart(function () {
            $spinner.show();
        });

        $(selectorClass).click(function () {
            if ($.active <= 0) {
                console.log('player selector click: ' + $(this).attr('data-userID'));
                $.fn.jpptimeline.loadUser($(this).attr('data-userID'));
            }
            else
                console.log('Active ajax request: Could not process request');
        });


        return this;

    };

}(jQuery));
