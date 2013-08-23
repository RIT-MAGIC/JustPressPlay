/*
 * JPP Image List - Jquery plugin
 * Handles the creation and update of an Image List
 * Just Press Play (http://play.rit.edu/)
 * 2013
 * Probably on a MIT license
 */


// TODO: Minifiy!

(function ($) {

    $.fn.jppimagelist = function (options) {

        var self = $(this);
        var listClass = 'imageList';
        var itemClass = 'imageListItem';
        var loadingClass = 'loading';
        var $list;
        var $bottom;
        var $spinner;
        var displayCount = 0;                           // The current number of items displayed
        var ajaxData = null;
        var scrollBuffer = 100;

        // Create settings object
        var settings = $.extend({

            // List Content
            baseURL: null,
            userID: null,
            achievementID: null,
            questID: null,
            achievementList: false,
            questList: false,
            playerList: false,
            showHeading: false,
            includeText: false,

            // Filters
            earned: null,
            tracked: null,
            friendsWith: null,
            publicPlayers: true,

            // Styles
            scroll: false,
            largeSize: null,
            smallSize: 4,

            // Load Intervals
            startIndex: 0,
            loadInterval: 28
        }, options);


        /* FUNCTIONS */

        // Loads items based upon settings object, appends to parent div
        $.fn.jppimagelist.load = function () {

            console.log('Load call');

            settings.startIndex = displayCount;


            if (displayCount % settings.loadInterval == 0)
                $spinner.show();

            $.ajax({
                url: buildAjaxURL(),
                dataType: "json",
                type: "POST",
                success: function (data) {

                    // Determine the type of list to build
                    if (settings.achievementList) {

                        // Store data as Achievement
                        ajaxData = data.Achievements;

                        if (displayCount <= 0 && settings.showHeading) self.prepend('<h4>ACHIEVEMENTS<span> - ' + data.Total + '</span></h4>');
                        

                        // Add to list

                        
                        generateListItems(buildAchievement);
                    }
                    else if (settings.questList) {

                        // Store data as Quest
                        ajaxData = data.Quests;

                        if (displayCount <= 0 && settings.showHeading) self.prepend('<h4>QUESTS<span> - ' + data.Total + '</span></h4>');

                        
                        generateListItems(buildQuest);
                    }
                    else if (settings.playerList) {

                        // Store data as People
                        ajaxData = data.People;


                        if (displayCount <= 0 && settings.showHeading)
                            self.prepend('<h4>' + (settings.friendsWith == true ? 'FRIENDS' : 'PUBLIC' ) + '<span> - ' + data.Total + '</span></h4>');

                        
                        generateListItems(buildPlayer);
                    }
                

                    $spinner.hide();

                    // Clear loaded data
                    delete ajaxData;

                },
                error: function (e) {

                    $spinner.hide();

                    self.append('<div class="timelinePost">' +
                                    '<h5>Whoops! Something went wrong</h5>' +
                                '</div>');

                    return false;
                }
            });

            // Don't place anything here; ajax is async and we don't know when it will finish
        };




        /*#region Build Functions*/

        // Builds the URL to be queried by Ajax
        // RETURNS: URL String
        var buildAjaxURL = function() {
            var query = settings.baseURL + '/JSON/';

            if (settings.achievementList) query += 'Achievements?';
            else if (settings.questList) query += 'Quests?';
            else if (settings.playerList) query += 'Players?';
            else {
                console.log('jppimagelist: ERROR: MISSING LIST TYPE');
                return null;
            }

            

            if(settings.questID != null)
                query += 'earnedQuest=' + settings.questID + '&';

            if(settings.achievementID != null)
                query += 'earnedAchievement=' + settings.achievementID + '&';

            if(settings.userID != null)
                query += 'userID=' + settings.userID + '&';

            if(settings.startIndex != null && settings.startIndex >= 0)
                query += 'start=' + settings.startIndex + '&';

            if(settings.loadInterval != null && settings.loadInterval >= 0)
                query += 'count=' + settings.loadInterval + '&';

            // Check for type
            if (settings.earned && settings.userID != null)
                query += 'achievementsEarned=true&';

            if (settings.friendsWith && settings.playerList && settings.userID != null)
                query += 'friendsWith=true&';
            if (settings.friendsWith == false && settings.playerList && settings.userID != null)
                query += 'friendsWith=false&';
            if (settings.friendsWith == null)
                query += 'friendsWith=null&';

            return query;
        };


        // Calls buildFunction and inserts each list item for a given query
        var generateListItems = function (buildFunction) {

            // For the number of items to build
            for (var i = 0; i < (settings.loadInterval < ajaxData.length ? settings.loadInterval : ajaxData.length) ; i++) {

                // Build and append each item
                $list.append(buildFunction(ajaxData[i]));

                ++displayCount;

            }

            if (ajaxData.length > 0) {
                //$(self).append('<div class="timelinePost"><p>This user hasn\'t earned this item yet<p></div>');
                console.log("bind scroll");
                
                if (settings.scroll)
                {
                    // TODO: Bind scroll event to 'this' or 'self'
                    $list.bind('scroll', imagelistScroll);
                }
                else
                {
                    $(window).bind('scroll', imagelistScroll);
                }
            }
            if (ajaxData.length == 0 && displayCount <= 0) {
                $bottom.before('<p style="margin: 0;">There\'s nothing here!</p>');

                if (settings.scroll)
                    self.children('.slimScrollDiv').remove();
                else
                    self.remove();
                ++displayCount;
            }

            
        };


        // 
        var buildAchievement = function (achievement) {

            // Create list element
            var $listItem = $(document.createElement('li')).addClass(itemClass);

            // Build base link to achievement...
            var url = settings.baseURL + '/Achievements/' + achievement.ID;

            // ...and link to a user's earning(s) if a userID is supplied
            if (settings.userID != null) url += '#' + settings.userID;


            // Build and add the link
            $listItem.append(   '<a href="' + url + '" title="' + achievement.Title + '">' +
                                    '<div class="imageContainer">' +
                                        '<div class="achievement">' +
                                            '<div class="createQuad"></div>' +
                                            '<div class="exploreQuad"></div>' +
                                            '<div class="learnQuad"></div>' +
                                            '<div class="socialQuad"></div>' +
                                        '</div>' +
                                        (achievement.PointsLearn > 0 ? '<div class="learnQuad"></div>' : '') +
                                        (achievement.PointsCreate > 0 ? '<div class="createQuad"></div>' : '') +
                                        (achievement.PointsExplore > 0 ? '<div class="exploreQuad"></div>' : '') +
                                        (achievement.PointsSocialize > 0 ? '<div class="socialQuad"></div>' : '') +
                                        '<img src="' + achievement.Image.substr(1) + '" />' +
                                    '</div>' +
                                    (settings.includeText ? '<p>' + achievement.Title + '</p>' : '') +
                                '</a>');

            return $listItem;
        };



        var buildQuest = function (quest) {
            var $listItem = $(document.createElement('li')).addClass(itemClass);

            // Build base link to achievement...
            var url = settings.baseURL + '/Quests/' + quest.ID;

            // ...and link to a user's earning(s) if a userID is supplied
            if (settings.userID != null) url += '#' + settings.userID;

            $listItem.append('<a href="' + url + '" title="' + quest.Title + '">' +
                                    '<div class="imageContainer sysQuest"><img src="' + quest.Image.substr(1) + '" /></div>' +
                                    (settings.includeText ? '<p>' + quest.Title + '</p>' : '') +
                                '</a>');

            return $listItem;
        };



        var buildPlayer = function (player) {
            var $listItem = $(document.createElement('li')).addClass(itemClass);

            // Hash the url if we are on an achievement page
            var url = settings.baseURL + (settings.achievementID != null ?
                            '/Achievements/' + settings.achievementID + '#' + player.ID :
                            '/Players/' + player.ID);

            // Build the url path for the user's photo
            var imageSrc = '';

            if (player.Image == null) {
                imageSrc = '/Content/Images/Jpp/defaultProfileAvatar.png';
            }
            else {
                imageSrc = player.Image.substr(1);
                imageSrc = imageSrc.replace(/\.([^.]+)$/, '_m.$1');
            }

            $listItem.append('<a href="' + url + '" title="' + player.DisplayName + '">' +
                                    '<div class="imageContainer player"><img src="' + imageSrc + '" /></div>' +
                                    (settings.includeText ? '<p>' + player.DisplayName + '</p>' : '') +
                                '</a>');

            return $listItem;
        };
        

        /*#endregion*/


        
        // Callback for scroll event to handle additional loading
        var imagelistScroll = function () {

            console.log('imagelistScroll');

            if (settings.scroll) {
                if ($list.scrollTop() >= $list.innerHeight - scrollBuffer) {
                    self.unbind('scroll');
                    $.fn.jppimagelist.load();
                }
            }
            else {

                console.log('imagelistScroll - settings.scroll == false');

                if ($(window).scrollTop() + $(window).height() >= self[0].scrollHeight + self.offset().top - scrollBuffer) {
                    $(window).unbind('scroll');
                    $.fn.jppimagelist.load();
                }
            }

            // Check for a scroll to the bottom of the timelineFeed - scrollBuffer
            
        };
        



        // INIT
        var init = function () {

            displayCount = 0;

            // Create the list containing parent
            $list = $(document.createElement('ul')).addClass('small-block-grid-' + settings.smallSize).addClass(listClass);

            // Apply optional styles
            if (settings.largeSize != null) $list.addClass('large-block-grid-' + settings.largeSize);
            if (settings.scroll) $list.addClass('scroll');
            self.addClass('gridContainer');

            self.append($list);

            // Add bottom
            var $bot = $(document.createElement('div')).addClass('bottom');
            $bot.append('<div class="spinner small"></div>');
            self.append($bot);
            $bottom = self.children('.bottom');
            $spinner = $bottom.children('.spinner');

            if (settings.startIndex == null) settings.startIndex = displayCount;

            // Initial load
            $.fn.jppimagelist.load();
        }

        init();


        $($spinner).ajaxStart(function () {
            $($spinner).show();
        });


        return this;

    };

}(jQuery));
