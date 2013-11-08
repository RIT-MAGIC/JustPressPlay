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
        var $top;
        var $bottom;
        var $spinner;
        var displayCount = 0;                           // The current number of items displayed
        var ajaxData = null;
        var scrollBuffer = 100;

        // External settings for imagelist
        var settings = $.extend({

            // List Content
            baseURL: null,
            userID: null,
            achievementID: null,
            questID: null,
            achievementList: false,
            questList: false,
            playerList: false,

            // Display Settings
            largeSize: null,
            smallSize: 4,
            displayListHeading: false,
            displayItemTitle: false,
            displayPrivacyChoice: false,
            scroll: false,

            // Filters
            earned: null,
            tracked: null,
            friendsWith: null,
            publicPlayers: true,

            // Load Intervals
            startIndex: 0,
            loadInterval: 28,

            debug: false
        }, options);


        // Initialization for an image list
        // Creates containing DOM elements and applies selected styles
        var init = function () {

            displayCount = 0;

            // Create the list containing parent
            $list = $(document.createElement('ul')).addClass('small-block-grid-' + settings.smallSize).addClass(listClass);

            // Apply optional styles
            if (settings.largeSize != null) $list.addClass('large-block-grid-' + settings.largeSize);
            if (settings.scroll) $list.addClass('scroll');
            //self.addClass('gridContainer');

            self.append($list);

            if (settings.displayPrivacyChoice)
            {
                settings.friendsWith = true;
                $top = $(document.createElement('form')).addClass('privacySelect');
                $top.append('<input type="radio" id="friendsSelect" name="privacySelect" value="Friends" checked />' +
                            '<label for="friendsSelect">Friends</label>' +
                            '<input type="radio" id="publicSelect" name="privacySelect" value="Public" />' +
                            '<label for="publicSelect">Public</label>');
                self.before($top);


                $("input[name='privacySelect']").change(function () {
                    if ($('#friendsSelect').is(':checked'))
                    {
                        settings.friendsWith = true;
                    }
                    else
                    {
                        settings.friendsWith = null;
                    }

                    // Reload list
                    displayCount = 0;
                    $list.empty();
                    $.fn.jppimagelist.load();
                });
            }


            // Add bottom & spinner
            var $bot = $(document.createElement('div')).addClass('bottom');
            $bot.append('<div class="spinner small"></div>');
            self.append($bot);
            $bottom = self.children('.bottom');
            $spinner = $bottom.children('.spinner');

            if (settings.startIndex == null) settings.startIndex = displayCount;

            // Initial load
            $.fn.jppimagelist.load();
        }


        /* FUNCTIONS */

        // Generates a valid image path for a supplied src
        // @param imgSrc Base url image can be found at
        // @param size Optional desired size (s, m)
        // @return Valid url for the given src and size
        var getImageURL = function (imgSrc, size) {
            var imageSrc = '';

            if (imgSrc == null) {
                imageSrc = '/Content/Images/Jpp/defaultProfileAvatar.png';
            }
            else {
                imageSrc = imgSrc.replace(/\\/g, "/").substr(1);
                if (size != null) imageSrc.replace(/\.([^.]+)$/, '_' + size + '.$1');
            }

            return imageSrc;
        }



        // Loads items based upon settings object, appends to parent div
        $.fn.jppimagelist.load = function () {

            if( settings.debug ) console.log('Load call');

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

                        if (displayCount <= 0 && settings.displayListHeading)
                            self.prepend('<h4>ACHIEVEMENTS<span> - ' + data.Total + '</span></h4>');
                        
                        generateListItems(buildAchievement);
                    }
                    else if (settings.questList) {

                        // Store data as Quest
                        ajaxData = data.Quests;

                        if (displayCount <= 0 && settings.displayListHeading)
                            self.prepend('<h4>QUESTS<span> - ' + data.Total + '</span></h4>');

                        
                        generateListItems(buildQuest);
                    }
                    else if (settings.playerList) {

                        // Store data as People
                        ajaxData = data.People;


                        if (displayCount <= 0 && settings.displayListHeading)
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

        var reload = function () {
            displayCount = 0;
            $list.empty();
            self.load();
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
                if ( settings.debug ) console.log('jppimagelist: ERROR: MISSING LIST TYPE');
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

                if ( settings.debug ) console.log("bind scroll");
                
                if ( settings.scroll )
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


        // Generates an individual achievement item
        // @return $listItem A jQuery object
        var buildAchievement = function (achievement) {

            // Create list element
            var $listItem = $(document.createElement('li')).addClass(itemClass);

            // Build base link to achievement...
            var url = settings.baseURL + '/Achievements/' + achievement.ID;
            var imageSrc = getImageURL(achievement.Image, 'm');

            // ...and link to a user's earning(s) if a userID is supplied
            if (settings.userID != null) url += '#' + settings.userID;


            // Build and add the link
            $listItem.append(   '<a href="' + url + '" title="' + achievement.Title + '">' +
                                    '<img src="' + imageSrc + '" />' +
                                    (settings.displayItemTitle ? '<p>' + achievement.Title + '</p>' : '') +
                                '</a>');

            return $listItem;
        };


        // Generates an individual quest item
        // @return $listItem A jQuery object
        var buildQuest = function (quest) {
            var $listItem = $(document.createElement('li')).addClass(itemClass);

            // Build base link to achievement...
            var url = settings.baseURL + '/Quests/' + quest.ID;
            var imageSrc = getImageURL(quest.Image, 'm');

            // ...and link to a user's earning(s) if a userID is supplied
            if (settings.userID != null) url += '#' + settings.userID;


            // Build and add the link
            $listItem.append(   '<a href="' + url + '" title="' + quest.Title + '">' +
                                    '<img src="' + imageSrc + '" />' +
                                    (settings.displayItemTitle ? '<p>' + quest.Title + '</p>' : '') +
                                '</a>');

            return $listItem;
        };


        // Generates an individual player item
        // @return $listItem A jQuery object
        var buildPlayer = function (player) {
            var $listItem = $(document.createElement('li')).addClass(itemClass);

            // Hash the url if we are on an achievement page
            var url = settings.baseURL + (settings.achievementID != null ?
                            '/Achievements/' + settings.achievementID + '#' + player.ID :
                            '/Players/' + player.ID);

            // Build the url path for the user's photo
            var imageSrc = getImageURL(player.Image, 'm');

            $listItem.append(   '<a href="' + url + '" title="' + player.DisplayName + '">' +
                                    '<img src="' + imageSrc + '" />' +
                                    (settings.displayItemTitle ? '<p>' + player.DisplayName + '</p>' : '') +
                                '</a>');

            return $listItem;
        };
        

        /*#endregion*/


        
        // Callback for scroll event to handle additional loading
        var imagelistScroll = function () {

            if ( settings.debug ) console.log('imagelistScroll');

            if (settings.scroll) {
                if ($list.scrollTop() >= $list.innerHeight - scrollBuffer) {
                    self.unbind('scroll');
                    $.fn.jppimagelist.load();
                }
            }
            else {

                if ( settings.debug ) console.log('imagelistScroll - settings.scroll == false');

                if ($(window).scrollTop() + $(window).height() >= self[0].scrollHeight + self.offset().top - scrollBuffer) {
                    $(window).unbind('scroll');
                    $.fn.jppimagelist.load();
                }
            }

            // Check for a scroll to the bottom of the timelineFeed - scrollBuffer
            
        };
 


        init();


        $($spinner).ajaxStart(function () {
            $($spinner).show();
        });


        return this;

    };

}(jQuery));
