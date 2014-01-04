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

    self.rawEarnedDate = data.EarnedDate;
    self.rawImage = data.Image;
    self.rawPlayerImage = data.PlayerImage;

    self.image = getImageURL(self.rawImage);
    self.playerImage = getImageURL(self.rawPlayerImage, 's');
    self.earnedDate = new Date(parseInt(data.EarnedDate.substr(6))).toLocaleDateString()
    
}

function EarningListViewModel(playerID) {
    var self = this;

    // Options
    self.loadInterval = 6;
    self.loadCount = 0;
    //TODO: Check for null playerID
    self.playerID = playerID;

    // Dynamic data
    self.earnings = ko.observableArray();

    // Functions
    //TODO: Load intervals and keep count
    //TODO: Track scroll
    self.loadEarnings = function () {
        $.get("/JSON/Earnings", { id: playerID }).done(function (data) {
            for (var i = 0; i < data.Earnings.length; i++) {
                self.earnings.push(new Earning(data.Earnings[i]));
            }
        });
    };

    self.loadEarnings();
}