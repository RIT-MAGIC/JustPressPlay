// jppviewmodels.js
//
// Defines various viewmodels for JPP

//TODO: Style templates (start with badge)
//TODO: Look into viewmodel duplication (per page, for feed duplication)
//TODO: Edit .spinner and .bottom selectors for closest feed

// Generates a valid image path for a supplied src
// @param imgSrc Base url image can be found at
// @param size Optional desired size (s, m)
// @param profile Should a null src be replaced with the default profile image?
// @return Valid url for the given src and size
var cleanImageURL = function (imgSrc, size) {
    var imageSrc = '';

    if (imgSrc === null) {
        imageSrc = null;
    }
    else {
        // Replace back-slashes, and remove tilda from beginning of string
        imageSrc = imgSrc.replace(/\\/g, "/").substr(1);
        if (size === 's' || size === 'm') {
            imageSrc = imageSrc.replace(/\.([^.]+)$/, '_' + size + '.$1');
        }
    }

    return imageSrc;
}

// Builds an object containing all information relating to an earning instance
// @param data JSON data representing the earning
function Earning(data) {
    var self = this;
    self.playerID = data.PlayerID;
    self.templateID = data.TemplateID;
    self.earningID = data.EarningID;
    self.displayName = data.DisplayName;
    self.title = data.Title;
    self.earningIsAchievement = data.EarningIsAchievement;
    self.contentPhoto = cleanImageURL(data.ContentPhoto, null);;
    self.contentText = data.ContentText;
    self.contentURL = data.ContentURL;
    self.commentsDisabled = data.CommentsDisabled;
    self.comments = data.comments;
    self.storyPhoto = cleanImageURL(data.StoryPhoto, null);
    self.storyText = data.StoryText;
    self.image = cleanImageURL(data.Image, 'm');
    self.playerImage = cleanImageURL(data.PlayerImage, null);
    if (self.playerImage === null) self.playerImage = '/Content/Images/Jpp/defaultProfileAvatar.png';
    self.earnedDate = new Date(parseInt(data.EarnedDate.substr(6))).toLocaleDateString();
}

// ViewModel for the Earning List
// @param settings JSON list of options for data retrieval
function EarningListViewModel(settings) {
    var self = this;

    // Options
    self.loadInterval = 6;
    self.loadCount = 0;
    self.scrollBuffer = 400;
    self.playerID = settings.playerID;
    self.achievementID = settings.achievementID;
    self.questID = settings.questID;
    
    // Dynamic data
    self.earnings = ko.observableArray();

    // Functions
    // Retrieves earning data from server and appends it to the earning array
    self.loadEarnings = function () {
        // Only show the loading spinner if there is probably more stuff to load
        if (self.loadCount % self.loadInterval == 0) {
            //TODO: select closest .spinner
            $('.earningFeed .bottom .spinner').show();
        }
        else {
            //TODO: select closest .endOfFeed
            $('.earningFeed .bottom .endOfFeed').show();
        }
        
        // Ajax request
        $.get("/JSON/Earnings", {
                id: self.playerID,
                achievementID: self.achievementID,
                questID: self.questID,
                start: self.loadCount,
                count: self.loadInterval
        }).done(function (data) {

            var dataCount = data.Earnings.length;

            // Update loadCount
            self.loadCount += dataCount;

            // Build new earnings
            for (var i = 0; i < dataCount; i++) {
                self.earnings.push(new Earning(data.Earnings[i]));
            }

            // Bind scroll
            if (dataCount > 0) {
                $(window).bind('scroll', self.bindScroll);
            }
            else {
                //TODO: select closest .endOfFeed
                $('.earningFeed .bottom .endOfFeed').show();
            }

            //TODO: select closest .spinner
            $('.earningFeed .bottom .spinner').hide();
        });
    };

    // Callback for scroll event to handle additional loading
    //TODO: Bind scroll to parent to support multiple lists
    self.bindScroll = function () {
        // Check for a scroll to the bottom of the timelineFeed - scrollBuffer
        if ($(window).scrollTop() + $(window).height() >= $('.earningFeed')[0].scrollHeight + $('.earningFeed').offset().top - self.scrollBuffer) {
            $(window).unbind('scroll');
            self.loadEarnings();
        }
    };

    // Initial load
    self.loadEarnings();
}


function Achievement(data) {
    var self = this;
    self.ID = data.ID;
    self.image = cleanImageURL(data.Image, 'm');
    self.pointsCreate = data.PointsCreate;
    self.pointsExplore = data.PointsExplore;
    self.pointsLearn = data.PointsLearn;
    self.pointsSocialize = data.PointsSocialize;
    self.title = data.Title;
}

function AchievementListViewModel(settings) {
    var self = this;

    // Options
    self.lists = ['All', 'Earned', 'Locked'];
    self.playerID = settings.playerID;
    self.earnedAchievement = null;
    self.activeList = ko.observable();

    // Dynamic data
    self.achievements = ko.observableArray();

    // Functions
    // Retrieves achievement data from server and appends it to the earning array
    // TODO: Load 28 and then the rest to speed up load
    self.loadAchievements = function () {

        // Clear current list
        self.achievements.removeAll();

        // Show loading spinner
        $('.bottom .spinner').show();
        

        // Ajax request
        $.get("/JSON/Achievements", {
            userID: self.playerID,
            //start: 0,
            //count: 6
            achievementsEarned: self.earnedAchievement
        }).done(function (data) {

            var dataCount = data.Achievements.length;

            // Build new achievements
            for (var i = 0; i < dataCount; i++) {
                self.achievements.push(new Achievement(data.Achievements[i]));
            }

            self.achievements.sort(function (left, right) { return left.title == right.title ? 0 : (left.title < right.title ? -1 : 1) });

            // Empty message
            if (dataCount == 0) {
                //TODO: select closest .endOfFeed
                //$('.earningFeed .bottom .endOfFeed').show();
            }

            //TODO: select closest .spinner
            $('.bottom .spinner').hide();
        });
    };

    self.loadList = function (list) {
        if (list !== self.activeList()) {

            self.activeList(list);
            switch (list) {
                case self.lists[0]:
                    self.earnedAchievement = null;
                    break;
                case self.lists[1]:
                    self.earnedAchievement = true;
                    break;
                case self.lists[2]:
                    self.earnedAchievement = false;
                    break;
                default:
                    self.earnedAchievement = null;
                    break;
            }

            self.loadAchievements();
        }
    }

    self.filterCreate = function() {

    }

    // Initial load
    self.loadList('All');
}