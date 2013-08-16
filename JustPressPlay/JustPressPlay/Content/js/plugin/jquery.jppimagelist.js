/*
 * JPP Image List - Jquery plugin
 * Handles the creation and update of an Image List
 * Built by Brandon Littell for Just Press Play (http://play.rit.edu/)
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
            userID: null,
            achievementID: null,
            questID: null,
            achievementList: false,
            questList: false,
            playerList: false,
            earned: null,
            tracked: null,
            friendsWith: null,
            showTitle: false,
            includeText: false,
            publicPlayers: true,
            baseURL: null,

            // Styles
            scroll: false,
            largeSize: null,
            smallSize: 4,

            // Load Intervals
            startIndex: 0,
            loadInterval: 30
        }, options);


        // INIT Function


        /* FUNCTIONS */

        // Loads items based upon settings object, appends to parent div
        $.fn.jppimagelist.load = function () {

            
            settings.startIndex = displayCount;
            $('.' + loadingClass).show();

            $.ajax({
                url: buildAjaxURL(),
                dataType: "json",
                type: "POST",
                success: function (data) {

                    // Determine the type of list to build
                    if (settings.achievementList) {

                        if (settings.showTitle) self.append('<h4>ACHIEVEMENTS<span> - ' + data.Total + '</span></h4>');

                        ajaxData = data.Achievements;
                        buildList(buildAchievement);
                    }
                    else if (settings.questList) {
                        if (settings.showTitle) self.append('<h4>QUESTS<span> - ' + data.Total + '</span></h4>');

                        ajaxData = data.Quests;
                        buildList(buildQuest);
                    }
                    else if (settings.playerList) {
                        if (settings.showTitle)
                            self.append('<h4>' + (settings.friendsWith == true ? 'FRIENDS' : 'PUBLIC' ) + '<span> - ' + data.Total + '</span></h4>');

                        ajaxData = data.People;
                        buildList(buildPlayer);
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
                query += 'questID=' + settings.questID + '&';

            if(settings.achievementID != null)
                query += 'achievementID=' + settings.achievementID + '&';

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

            return query;
        };


        var buildList = function (buildFunction) {

            // Create the list containing parent
            var $list = $(document.createElement('ul')).addClass('small-block-grid-' + settings.smallSize).addClass(listClass);

            // Apply optional styles
            if (settings.largeSize != null) $list.addClass('large-block-grid-' + settings.largeSize);
            if (settings.scroll) $list.addClass('scroll');
            if (settings.showTitle) $list.addClass('gridContainer');


            // For the number of items to build
            for (var i = 0; i < (settings.loadInterval < ajaxData.length ? settings.loadInterval : ajaxData.length) ; i++) {

                // Build each and append each
                $list.append(buildFunction(ajaxData[i]));

                ++displayCount;

            }

            self.append($list);

            if (ajaxData.length <= 0) {
                //$(self).append('<h5>This user hasn\'t earned this item yet</h5>');
            }
            else {
                //console.log("bind scroll");
                //$(window).bind('scroll', bindScroll);
            }

            
        };



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

            $listItem.append('<a href="' + url + '" title="' + player.DisplayName + '">' +
                                    '<div class="imageContainer player"><img src="' + (player.Image != null ? player.Image.substr(1) : '/Content/Images/Jpp/defaultProfileAvatar.png') + '" /></div>' +
                                    (settings.includeText ? '<p>' + player.DisplayName + '</p>' : '') +
                                '</a>');

            return $listItem;
        };
        

        /*#endregion*/


        /*
        // Callback for scroll event to handle additional loading
        var bindScroll = function () {
            // Check for a scroll to the bottom of the timelineFeed - scrollBuffer
            if ($(window).scrollTop() + $(window).height() >= $(self)[0].scrollHeight + $(self).offset().top - scrollBuffer) {
                $(window).unbind('scroll');
                //$.fn.jppimagelist.load();
            }
        };
        */



        // INIT
        var init = function () {

            // Create feed container
            var $tempFeed = $(document.createElement('div')).addClass('timelineFeed');
            self.append($tempFeed);
            $list = self.children('.timelineFeed');

            // Add bottom
            var $bot = $(document.createElement('div')).addClass('bottom');
            $bot.append('<div class="spinner small"></div>');
            self.append($bot);
            $bottom = self.children('.bottom');
            $spinner = $bottom.children('.spinner');

            // Initial load
            $.fn.jppimagelist.load();
        }

        init();


        $("#loading").ajaxStart(function () {
            $("#loading").show();
        });


        return this;

    };

}(jQuery));
