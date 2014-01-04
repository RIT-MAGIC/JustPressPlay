// jppviewmodels.js
//
//

//TODO: Style templates (start with badge)
//TODO: Look into viewmodel duplication (per page, for feed duplication)
//TODO: Edit .spinner and .bottom selectors for closest feed

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

//TODO: Document
function Earning(data) {
    var self = this;
    self.playerID = data.PlayerID;
    self.templateID = data.TemplateID;
    self.earningID = data.EarningID;
    self.displayName = data.DisplayName;
    self.title = data.Title;
    self.earningIsAchievement = data.EarningIsAchievement;
    self.contentPhoto = data.ContentPhoto;
    self.contentText = data.ContentText;
    self.contentURL = data.ContentURL;
    self.commentsDisabled = data.CommentsDisabled;
    self.comments = data.comments;
    self.storyPhoto = data.StoryPhoto;
    self.storyText = data.StoryText;
    self.image = getImageURL(data.Image);
    self.playerImage = getImageURL(data.PlayerImage, 's');
    self.earnedDate = new Date(parseInt(data.EarnedDate.substr(6))).toLocaleDateString();
}

//TODO: Document
function EarningListViewModel(settings) {
    var self = this;

    // Options
    self.loadInterval = 6;
    self.loadCount = 0;
    self.scrollBuffer = 200;

    // Check for setting values
    //if(settings.playerID !== null )
    self.playerID = settings.playerID;
    self.achievementID = settings.achievementID;
    self.questID = settings.questID;
    

    // Dynamic data
    self.earnings = ko.observableArray();

    // Functions
    //TODO: Document
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
