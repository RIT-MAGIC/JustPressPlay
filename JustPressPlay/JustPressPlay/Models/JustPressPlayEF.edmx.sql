
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 06/27/2013 16:05:30
-- Generated from EDMX file: C:\Users\Chris\Documents\Projects\JustPressPlayV3\JustPressPlay\JustPressPlay\Models\JustPressPlayEF.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [JustPressPlayDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'user'
CREATE TABLE [dbo].[user] (
    [id] int IDENTITY(1,1) NOT NULL,
    [username] nvarchar(255)  NOT NULL,
    [password_with_salt] nvarchar(255)  NOT NULL,
    [salt] nvarchar(255)  NOT NULL,
    [first_name] nvarchar(255)  NULL,
    [middle_name] nvarchar(255)  NULL,
    [last_name] nvarchar(255)  NULL,
    [is_player] bit  NOT NULL,
    [created_date] datetime  NOT NULL,
    [status] int  NOT NULL,
    [first_login] bit  NOT NULL,
    [email] nvarchar(255)  NOT NULL,
    [last_login_date] datetime  NOT NULL,
    [organization_id] nvarchar(255)  NULL,
    [organization_program_code] nvarchar(255)  NULL,
    [organization_year_level] nvarchar(255)  NULL,
    [organization_user_type] nvarchar(255)  NULL,
    [display_name] nvarchar(255)  NOT NULL,
    [six_word_bio] nvarchar(255)  NULL,
    [full_bio] nvarchar(max)  NULL,
    [image] nvarchar(255)  NULL,
    [personal_url] nvarchar(255)  NULL,
    [privacy_settings] int  NOT NULL,
    [has_agreed_to_tos] bit  NOT NULL,
    [creator_id] int  NULL,
    [modified_date] datetime  NULL,
    [custom_1] nvarchar(255)  NULL,
    [custom_2] nvarchar(255)  NULL,
    [custom_3] nvarchar(255)  NULL
);
GO

-- Creating table 'achievement_template'
CREATE TABLE [dbo].[achievement_template] (
    [id] int IDENTITY(1,1) NOT NULL,
    [title] nvarchar(255)  NOT NULL,
    [description] nvarchar(max)  NOT NULL,
    [icon] nvarchar(255)  NOT NULL,
    [type] int  NOT NULL,
    [featured] bit  NOT NULL,
    [hidden] bit  NOT NULL,
    [state] int  NOT NULL,
    [parent_id] int  NULL,
    [threshold] int  NULL,
    [creator_id] int  NOT NULL,
    [created_date] datetime  NOT NULL,
    [posted_date] datetime  NULL,
    [retire_date] nvarchar(max)  NOT NULL,
    [modified_date] datetime  NULL,
    [last_modified_by_id] int  NULL,
    [content_type] int  NULL,
    [system_trigger_type] int  NULL,
    [repeat_delay_days] int  NULL,
    [points_create] int  NOT NULL,
    [points_explore] int  NOT NULL,
    [points_learn] int  NOT NULL,
    [points_socialize] int  NOT NULL
);
GO

-- Creating table 'achievement_instance'
CREATE TABLE [dbo].[achievement_instance] (
    [id] int IDENTITY(1,1) NOT NULL,
    [user_id] int  NOT NULL,
    [achievement_id] int  NOT NULL,
    [achieved_date] datetime  NOT NULL,
    [has_user_content] bit  NOT NULL,
    [user_content_id] int  NULL,
    [has_user_story] bit  NOT NULL,
    [user_story_id] int  NULL,
    [card_given] bit  NOT NULL,
    [card_given_date] datetime  NULL,
    [assigned_by_id] int  NOT NULL,
    [points_create] int  NOT NULL,
    [points_explore] int  NOT NULL,
    [points_learn] int  NOT NULL,
    [points_socialize] int  NOT NULL,
    [comments_disabled] bit  NOT NULL
);
GO

-- Creating table 'achievement_requirement'
CREATE TABLE [dbo].[achievement_requirement] (
    [id] int IDENTITY(1,1) NOT NULL,
    [achievement_id] int  NOT NULL,
    [description] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'achievement_caretaker'
CREATE TABLE [dbo].[achievement_caretaker] (
    [id] int IDENTITY(1,1) NOT NULL,
    [achievement_id] int  NOT NULL,
    [caretaker_id] int  NOT NULL
);
GO

-- Creating table 'achievement_user_content'
CREATE TABLE [dbo].[achievement_user_content] (
    [id] int IDENTITY(1,1) NOT NULL,
    [content_type] int  NOT NULL,
    [submitted_date] datetime  NOT NULL,
    [approved_date] datetime  NOT NULL,
    [approved_by_id] int  NOT NULL,
    [image] nvarchar(255)  NULL,
    [text] nvarchar(max)  NULL,
    [url] nvarchar(255)  NULL
);
GO

-- Creating table 'achievement_user_content_pending'
CREATE TABLE [dbo].[achievement_user_content_pending] (
    [id] int IDENTITY(1,1) NOT NULL,
    [achievement_id] int  NOT NULL,
    [content_type] int  NOT NULL,
    [submitted_by_id] int  NOT NULL,
    [submitted_date] datetime  NOT NULL,
    [image] nvarchar(255)  NULL,
    [text] nvarchar(max)  NULL,
    [url] nvarchar(255)  NULL
);
GO

-- Creating table 'achievement_user_story'
CREATE TABLE [dbo].[achievement_user_story] (
    [id] int IDENTITY(1,1) NOT NULL,
    [date_submitted] datetime  NOT NULL,
    [image] nvarchar(255)  NULL,
    [text] nvarchar(max)  NULL
);
GO

-- Creating table 'friend'
CREATE TABLE [dbo].[friend] (
    [id] int IDENTITY(1,1) NOT NULL,
    [source_id] int  NOT NULL,
    [destination_id] int  NOT NULL,
    [friended_date] datetime  NOT NULL,
    [request_date] datetime  NOT NULL
);
GO

-- Creating table 'friend_pending'
CREATE TABLE [dbo].[friend_pending] (
    [id] int IDENTITY(1,1) NOT NULL,
    [source_id] int  NOT NULL,
    [destination_id] int  NOT NULL,
    [request_date] datetime  NOT NULL,
    [ignored] bit  NOT NULL
);
GO

-- Creating table 'log'
CREATE TABLE [dbo].[log] (
    [id] int IDENTITY(1,1) NOT NULL,
    [timestamp] datetime  NOT NULL,
    [user_id] int  NOT NULL,
    [ip_address] nvarchar(255)  NOT NULL,
    [action] nvarchar(255)  NOT NULL,
    [value_1] nvarchar(max)  NULL,
    [value_2] nvarchar(max)  NULL
);
GO

-- Creating table 'notification'
CREATE TABLE [dbo].[notification] (
    [id] int IDENTITY(1,1) NOT NULL,
    [source_id] int  NOT NULL,
    [destination_id] int  NOT NULL,
    [date] datetime  NOT NULL,
    [message] nvarchar(255)  NOT NULL,
    [icon] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'quest_template'
CREATE TABLE [dbo].[quest_template] (
    [id] int IDENTITY(1,1) NOT NULL,
    [title] nvarchar(255)  NOT NULL,
    [description] nvarchar(max)  NOT NULL,
    [icon] nvarchar(255)  NOT NULL,
    [featured] bit  NOT NULL,
    [state] int  NOT NULL,
    [creator_id] int  NOT NULL,
    [created_date] datetime  NOT NULL,
    [posted_date] datetime  NULL,
    [retire_date] nvarchar(max)  NOT NULL,
    [last_modified_by_id] int  NULL,
    [last_modified_date] datetime  NULL,
    [threshold] int  NULL,
    [user_generated] bit  NOT NULL
);
GO

-- Creating table 'quest_achievement_step'
CREATE TABLE [dbo].[quest_achievement_step] (
    [id] int IDENTITY(1,1) NOT NULL,
    [quest_id] int  NOT NULL,
    [achievement_id] int  NOT NULL
);
GO

-- Creating table 'quest_instance'
CREATE TABLE [dbo].[quest_instance] (
    [id] int IDENTITY(1,1) NOT NULL,
    [user_id] int  NOT NULL,
    [quest_id] int  NOT NULL,
    [completed_date] datetime  NOT NULL,
    [comments_disabled] bit  NOT NULL
);
GO

-- Creating table 'quest_tracking'
CREATE TABLE [dbo].[quest_tracking] (
    [id] int IDENTITY(1,1) NOT NULL,
    [user_id] int  NOT NULL,
    [quest_id] int  NOT NULL
);
GO

-- Creating table 'system_stat'
CREATE TABLE [dbo].[system_stat] (
    [id] int IDENTITY(1,1) NOT NULL,
    [total_players] int  NOT NULL,
    [total_possible_achievements] int  NOT NULL,
    [total_attained_achievements] int  NOT NULL,
    [total_possible_quests] int  NOT NULL,
    [total_completed_quests] int  NOT NULL,
    [total_possible_points_create] int  NOT NULL,
    [total_possible_points_explore] int  NOT NULL,
    [total_possible_points_learn] int  NOT NULL,
    [total_possible_points_socialize] int  NOT NULL,
    [total_attained_points_create] int  NOT NULL,
    [total_attained_points_explore] int  NOT NULL,
    [total_attained_points_learn] int  NOT NULL,
    [total_attained_points_socialize] int  NOT NULL
);
GO

-- Creating table 'external_token'
CREATE TABLE [dbo].[external_token] (
    [id] int IDENTITY(1,1) NOT NULL,
    [user_id] int  NOT NULL,
    [source] nvarchar(255)  NOT NULL,
    [token] nvarchar(255)  NOT NULL,
    [created_date] datetime  NOT NULL,
    [expiration_date] datetime  NOT NULL
);
GO

-- Creating table 'search_keyword'
CREATE TABLE [dbo].[search_keyword] (
    [id] int IDENTITY(1,1) NOT NULL,
    [keyword] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'achievement_keyword'
CREATE TABLE [dbo].[achievement_keyword] (
    [id] int IDENTITY(1,1) NOT NULL,
    [keyword_id] int  NOT NULL,
    [achievement_id] int  NOT NULL
);
GO

-- Creating table 'quest_keyword'
CREATE TABLE [dbo].[quest_keyword] (
    [id] int IDENTITY(1,1) NOT NULL,
    [keyword_id] int  NOT NULL,
    [quest_id] int  NOT NULL
);
GO

-- Creating table 'comment'
CREATE TABLE [dbo].[comment] (
    [id] int IDENTITY(1,1) NOT NULL,
    [location_id] int  NOT NULL,
    [location_type] int  NOT NULL,
    [user_id] int  NOT NULL,
    [text] nvarchar(max)  NOT NULL,
    [deleted] bit  NOT NULL,
    [date] datetime  NOT NULL,
    [last_modified_by_id] int  NOT NULL,
    [last_modified_date] datetime  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'user'
ALTER TABLE [dbo].[user]
ADD CONSTRAINT [PK_user]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_template'
ALTER TABLE [dbo].[achievement_template]
ADD CONSTRAINT [PK_achievement_template]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_instance'
ALTER TABLE [dbo].[achievement_instance]
ADD CONSTRAINT [PK_achievement_instance]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_requirement'
ALTER TABLE [dbo].[achievement_requirement]
ADD CONSTRAINT [PK_achievement_requirement]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_caretaker'
ALTER TABLE [dbo].[achievement_caretaker]
ADD CONSTRAINT [PK_achievement_caretaker]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_user_content'
ALTER TABLE [dbo].[achievement_user_content]
ADD CONSTRAINT [PK_achievement_user_content]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_user_content_pending'
ALTER TABLE [dbo].[achievement_user_content_pending]
ADD CONSTRAINT [PK_achievement_user_content_pending]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_user_story'
ALTER TABLE [dbo].[achievement_user_story]
ADD CONSTRAINT [PK_achievement_user_story]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'friend'
ALTER TABLE [dbo].[friend]
ADD CONSTRAINT [PK_friend]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'friend_pending'
ALTER TABLE [dbo].[friend_pending]
ADD CONSTRAINT [PK_friend_pending]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'log'
ALTER TABLE [dbo].[log]
ADD CONSTRAINT [PK_log]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'notification'
ALTER TABLE [dbo].[notification]
ADD CONSTRAINT [PK_notification]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'quest_template'
ALTER TABLE [dbo].[quest_template]
ADD CONSTRAINT [PK_quest_template]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'quest_achievement_step'
ALTER TABLE [dbo].[quest_achievement_step]
ADD CONSTRAINT [PK_quest_achievement_step]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'quest_instance'
ALTER TABLE [dbo].[quest_instance]
ADD CONSTRAINT [PK_quest_instance]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'quest_tracking'
ALTER TABLE [dbo].[quest_tracking]
ADD CONSTRAINT [PK_quest_tracking]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'system_stat'
ALTER TABLE [dbo].[system_stat]
ADD CONSTRAINT [PK_system_stat]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'external_token'
ALTER TABLE [dbo].[external_token]
ADD CONSTRAINT [PK_external_token]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'search_keyword'
ALTER TABLE [dbo].[search_keyword]
ADD CONSTRAINT [PK_search_keyword]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'achievement_keyword'
ALTER TABLE [dbo].[achievement_keyword]
ADD CONSTRAINT [PK_achievement_keyword]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'quest_keyword'
ALTER TABLE [dbo].[quest_keyword]
ADD CONSTRAINT [PK_quest_keyword]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'comment'
ALTER TABLE [dbo].[comment]
ADD CONSTRAINT [PK_comment]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [source_id] in table 'friend'
ALTER TABLE [dbo].[friend]
ADD CONSTRAINT [FK_user_friend_source]
    FOREIGN KEY ([source_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_friend_source'
CREATE INDEX [IX_FK_user_friend_source]
ON [dbo].[friend]
    ([source_id]);
GO

-- Creating foreign key on [destination_id] in table 'friend'
ALTER TABLE [dbo].[friend]
ADD CONSTRAINT [FK_user_friend_destination]
    FOREIGN KEY ([destination_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_friend_destination'
CREATE INDEX [IX_FK_user_friend_destination]
ON [dbo].[friend]
    ([destination_id]);
GO

-- Creating foreign key on [source_id] in table 'friend_pending'
ALTER TABLE [dbo].[friend_pending]
ADD CONSTRAINT [FK_user_friend_pending_source]
    FOREIGN KEY ([source_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_friend_pending_source'
CREATE INDEX [IX_FK_user_friend_pending_source]
ON [dbo].[friend_pending]
    ([source_id]);
GO

-- Creating foreign key on [destination_id] in table 'friend_pending'
ALTER TABLE [dbo].[friend_pending]
ADD CONSTRAINT [FK_user_friend_pending_destination]
    FOREIGN KEY ([destination_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_friend_pending_destination'
CREATE INDEX [IX_FK_user_friend_pending_destination]
ON [dbo].[friend_pending]
    ([destination_id]);
GO

-- Creating foreign key on [user_id] in table 'log'
ALTER TABLE [dbo].[log]
ADD CONSTRAINT [FK_user_log]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_log'
CREATE INDEX [IX_FK_user_log]
ON [dbo].[log]
    ([user_id]);
GO

-- Creating foreign key on [destination_id] in table 'notification'
ALTER TABLE [dbo].[notification]
ADD CONSTRAINT [FK_user_notification_destination]
    FOREIGN KEY ([destination_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_notification_destination'
CREATE INDEX [IX_FK_user_notification_destination]
ON [dbo].[notification]
    ([destination_id]);
GO

-- Creating foreign key on [source_id] in table 'notification'
ALTER TABLE [dbo].[notification]
ADD CONSTRAINT [FK_user_notification_source]
    FOREIGN KEY ([source_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_notification_source'
CREATE INDEX [IX_FK_user_notification_source]
ON [dbo].[notification]
    ([source_id]);
GO

-- Creating foreign key on [creator_id] in table 'achievement_template'
ALTER TABLE [dbo].[achievement_template]
ADD CONSTRAINT [FK_user_achievement_template_creator]
    FOREIGN KEY ([creator_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_achievement_template_creator'
CREATE INDEX [IX_FK_user_achievement_template_creator]
ON [dbo].[achievement_template]
    ([creator_id]);
GO

-- Creating foreign key on [last_modified_by_id] in table 'achievement_template'
ALTER TABLE [dbo].[achievement_template]
ADD CONSTRAINT [FK_user_achievement_template_last_modified_by]
    FOREIGN KEY ([last_modified_by_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_achievement_template_last_modified_by'
CREATE INDEX [IX_FK_user_achievement_template_last_modified_by]
ON [dbo].[achievement_template]
    ([last_modified_by_id]);
GO

-- Creating foreign key on [parent_id] in table 'achievement_template'
ALTER TABLE [dbo].[achievement_template]
ADD CONSTRAINT [FK_achievement_template_parent_id]
    FOREIGN KEY ([parent_id])
    REFERENCES [dbo].[achievement_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_template_parent_id'
CREATE INDEX [IX_FK_achievement_template_parent_id]
ON [dbo].[achievement_template]
    ([parent_id]);
GO

-- Creating foreign key on [achievement_id] in table 'achievement_requirement'
ALTER TABLE [dbo].[achievement_requirement]
ADD CONSTRAINT [FK_achievement_template_requirement]
    FOREIGN KEY ([achievement_id])
    REFERENCES [dbo].[achievement_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_template_requirement'
CREATE INDEX [IX_FK_achievement_template_requirement]
ON [dbo].[achievement_requirement]
    ([achievement_id]);
GO

-- Creating foreign key on [achievement_id] in table 'achievement_caretaker'
ALTER TABLE [dbo].[achievement_caretaker]
ADD CONSTRAINT [FK_achievement_template_caretaker_achievement_id]
    FOREIGN KEY ([achievement_id])
    REFERENCES [dbo].[achievement_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_template_caretaker_achievement_id'
CREATE INDEX [IX_FK_achievement_template_caretaker_achievement_id]
ON [dbo].[achievement_caretaker]
    ([achievement_id]);
GO

-- Creating foreign key on [caretaker_id] in table 'achievement_caretaker'
ALTER TABLE [dbo].[achievement_caretaker]
ADD CONSTRAINT [FK_user_achievement_caretaker_user_id]
    FOREIGN KEY ([caretaker_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_achievement_caretaker_user_id'
CREATE INDEX [IX_FK_user_achievement_caretaker_user_id]
ON [dbo].[achievement_caretaker]
    ([caretaker_id]);
GO

-- Creating foreign key on [approved_by_id] in table 'achievement_user_content'
ALTER TABLE [dbo].[achievement_user_content]
ADD CONSTRAINT [FK_user_achievement_user_content_approved_by]
    FOREIGN KEY ([approved_by_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_achievement_user_content_approved_by'
CREATE INDEX [IX_FK_user_achievement_user_content_approved_by]
ON [dbo].[achievement_user_content]
    ([approved_by_id]);
GO

-- Creating foreign key on [achievement_id] in table 'achievement_user_content_pending'
ALTER TABLE [dbo].[achievement_user_content_pending]
ADD CONSTRAINT [FK_achievement_template_user_content_pending]
    FOREIGN KEY ([achievement_id])
    REFERENCES [dbo].[achievement_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_template_user_content_pending'
CREATE INDEX [IX_FK_achievement_template_user_content_pending]
ON [dbo].[achievement_user_content_pending]
    ([achievement_id]);
GO

-- Creating foreign key on [submitted_by_id] in table 'achievement_user_content_pending'
ALTER TABLE [dbo].[achievement_user_content_pending]
ADD CONSTRAINT [FK_user_achievement_user_content_pending_submitted_by_id]
    FOREIGN KEY ([submitted_by_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_achievement_user_content_pending_submitted_by_id'
CREATE INDEX [IX_FK_user_achievement_user_content_pending_submitted_by_id]
ON [dbo].[achievement_user_content_pending]
    ([submitted_by_id]);
GO

-- Creating foreign key on [user_id] in table 'achievement_instance'
ALTER TABLE [dbo].[achievement_instance]
ADD CONSTRAINT [FK_user_achievement_instance]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_achievement_instance'
CREATE INDEX [IX_FK_user_achievement_instance]
ON [dbo].[achievement_instance]
    ([user_id]);
GO

-- Creating foreign key on [achievement_id] in table 'achievement_instance'
ALTER TABLE [dbo].[achievement_instance]
ADD CONSTRAINT [FK_achievement_template_instance]
    FOREIGN KEY ([achievement_id])
    REFERENCES [dbo].[achievement_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_template_instance'
CREATE INDEX [IX_FK_achievement_template_instance]
ON [dbo].[achievement_instance]
    ([achievement_id]);
GO

-- Creating foreign key on [user_content_id] in table 'achievement_instance'
ALTER TABLE [dbo].[achievement_instance]
ADD CONSTRAINT [FK_achievement_user_content_achievement_instance]
    FOREIGN KEY ([user_content_id])
    REFERENCES [dbo].[achievement_user_content]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_user_content_achievement_instance'
CREATE INDEX [IX_FK_achievement_user_content_achievement_instance]
ON [dbo].[achievement_instance]
    ([user_content_id]);
GO

-- Creating foreign key on [user_story_id] in table 'achievement_instance'
ALTER TABLE [dbo].[achievement_instance]
ADD CONSTRAINT [FK_achievement_user_story_achievement_instance]
    FOREIGN KEY ([user_story_id])
    REFERENCES [dbo].[achievement_user_story]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_user_story_achievement_instance'
CREATE INDEX [IX_FK_achievement_user_story_achievement_instance]
ON [dbo].[achievement_instance]
    ([user_story_id]);
GO

-- Creating foreign key on [assigned_by_id] in table 'achievement_instance'
ALTER TABLE [dbo].[achievement_instance]
ADD CONSTRAINT [FK_user_achievement_instance_assigned_by]
    FOREIGN KEY ([assigned_by_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_achievement_instance_assigned_by'
CREATE INDEX [IX_FK_user_achievement_instance_assigned_by]
ON [dbo].[achievement_instance]
    ([assigned_by_id]);
GO

-- Creating foreign key on [keyword_id] in table 'achievement_keyword'
ALTER TABLE [dbo].[achievement_keyword]
ADD CONSTRAINT [FK_search_keyword_achievement]
    FOREIGN KEY ([keyword_id])
    REFERENCES [dbo].[search_keyword]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_search_keyword_achievement'
CREATE INDEX [IX_FK_search_keyword_achievement]
ON [dbo].[achievement_keyword]
    ([keyword_id]);
GO

-- Creating foreign key on [achievement_id] in table 'achievement_keyword'
ALTER TABLE [dbo].[achievement_keyword]
ADD CONSTRAINT [FK_achievement_template_keyword]
    FOREIGN KEY ([achievement_id])
    REFERENCES [dbo].[achievement_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_template_keyword'
CREATE INDEX [IX_FK_achievement_template_keyword]
ON [dbo].[achievement_keyword]
    ([achievement_id]);
GO

-- Creating foreign key on [quest_id] in table 'quest_keyword'
ALTER TABLE [dbo].[quest_keyword]
ADD CONSTRAINT [FK_quest_template_keyword]
    FOREIGN KEY ([quest_id])
    REFERENCES [dbo].[quest_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_quest_template_keyword'
CREATE INDEX [IX_FK_quest_template_keyword]
ON [dbo].[quest_keyword]
    ([quest_id]);
GO

-- Creating foreign key on [keyword_id] in table 'quest_keyword'
ALTER TABLE [dbo].[quest_keyword]
ADD CONSTRAINT [FK_search_keyword_quest_keyword]
    FOREIGN KEY ([keyword_id])
    REFERENCES [dbo].[search_keyword]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_search_keyword_quest_keyword'
CREATE INDEX [IX_FK_search_keyword_quest_keyword]
ON [dbo].[quest_keyword]
    ([keyword_id]);
GO

-- Creating foreign key on [user_id] in table 'quest_tracking'
ALTER TABLE [dbo].[quest_tracking]
ADD CONSTRAINT [FK_user_quest_tracking]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_quest_tracking'
CREATE INDEX [IX_FK_user_quest_tracking]
ON [dbo].[quest_tracking]
    ([user_id]);
GO

-- Creating foreign key on [quest_id] in table 'quest_tracking'
ALTER TABLE [dbo].[quest_tracking]
ADD CONSTRAINT [FK_quest_template_tracking]
    FOREIGN KEY ([quest_id])
    REFERENCES [dbo].[quest_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_quest_template_tracking'
CREATE INDEX [IX_FK_quest_template_tracking]
ON [dbo].[quest_tracking]
    ([quest_id]);
GO

-- Creating foreign key on [quest_id] in table 'quest_achievement_step'
ALTER TABLE [dbo].[quest_achievement_step]
ADD CONSTRAINT [FK_quest_template_achievement_step]
    FOREIGN KEY ([quest_id])
    REFERENCES [dbo].[quest_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_quest_template_achievement_step'
CREATE INDEX [IX_FK_quest_template_achievement_step]
ON [dbo].[quest_achievement_step]
    ([quest_id]);
GO

-- Creating foreign key on [achievement_id] in table 'quest_achievement_step'
ALTER TABLE [dbo].[quest_achievement_step]
ADD CONSTRAINT [FK_achievement_template_quest_achievement_step]
    FOREIGN KEY ([achievement_id])
    REFERENCES [dbo].[achievement_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_achievement_template_quest_achievement_step'
CREATE INDEX [IX_FK_achievement_template_quest_achievement_step]
ON [dbo].[quest_achievement_step]
    ([achievement_id]);
GO

-- Creating foreign key on [user_id] in table 'quest_instance'
ALTER TABLE [dbo].[quest_instance]
ADD CONSTRAINT [FK_user_quest_instance]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_quest_instance'
CREATE INDEX [IX_FK_user_quest_instance]
ON [dbo].[quest_instance]
    ([user_id]);
GO

-- Creating foreign key on [quest_id] in table 'quest_instance'
ALTER TABLE [dbo].[quest_instance]
ADD CONSTRAINT [FK_quest_template_quest_instance]
    FOREIGN KEY ([quest_id])
    REFERENCES [dbo].[quest_template]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_quest_template_quest_instance'
CREATE INDEX [IX_FK_quest_template_quest_instance]
ON [dbo].[quest_instance]
    ([quest_id]);
GO

-- Creating foreign key on [creator_id] in table 'quest_template'
ALTER TABLE [dbo].[quest_template]
ADD CONSTRAINT [FK_user_quest_template_creator]
    FOREIGN KEY ([creator_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_quest_template_creator'
CREATE INDEX [IX_FK_user_quest_template_creator]
ON [dbo].[quest_template]
    ([creator_id]);
GO

-- Creating foreign key on [last_modified_by_id] in table 'quest_template'
ALTER TABLE [dbo].[quest_template]
ADD CONSTRAINT [FK_user_quest_template_last_modified_by]
    FOREIGN KEY ([last_modified_by_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_quest_template_last_modified_by'
CREATE INDEX [IX_FK_user_quest_template_last_modified_by]
ON [dbo].[quest_template]
    ([last_modified_by_id]);
GO

-- Creating foreign key on [user_id] in table 'external_token'
ALTER TABLE [dbo].[external_token]
ADD CONSTRAINT [FK_user_external_token]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_external_token'
CREATE INDEX [IX_FK_user_external_token]
ON [dbo].[external_token]
    ([user_id]);
GO

-- Creating foreign key on [user_id] in table 'comment'
ALTER TABLE [dbo].[comment]
ADD CONSTRAINT [FK_user_comment_poster]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_comment_poster'
CREATE INDEX [IX_FK_user_comment_poster]
ON [dbo].[comment]
    ([user_id]);
GO

-- Creating foreign key on [last_modified_by_id] in table 'comment'
ALTER TABLE [dbo].[comment]
ADD CONSTRAINT [FK_user_comment_last_modified_by]
    FOREIGN KEY ([last_modified_by_id])
    REFERENCES [dbo].[user]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_user_comment_last_modified_by'
CREATE INDEX [IX_FK_user_comment_last_modified_by]
ON [dbo].[comment]
    ([last_modified_by_id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------