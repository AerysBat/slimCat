﻿#region Copyright

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageListsViewModel.cs">
//    Copyright (c) 2013, Justin Kadrovach, All rights reserved.
//   
//    This source is subject to the Simplified BSD License.
//    Please see the License.txt file for more information.
//    All other rights reserved.
//    
//    THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
//    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//    PARTICULAR PURPOSE.
// </copyright>
//  --------------------------------------------------------------------------------------------------------------------

#endregion

namespace slimCat.ViewModels
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.Prism.Events;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.Unity;
    using Models;
    using Utilities;
    using Views;

    #endregion

    /// <summary>
    ///     The manage lists view model.
    /// </summary>
    public class ManageListsViewModel : ChannelbarViewModelCommon
    {
        #region Constants

        public const string ManageListsTabView = "ManageListsTabView";

        #endregion

        #region Fields

        private readonly GenderSettingsModel genderSettings;

        private bool showOffline;

        private readonly IDictionary<ListKind, string> listKinds = new Dictionary<ListKind, string>
            {
                {ListKind.Banned, "Banned"},
                {ListKind.Bookmark, "Bookmarks"},
                {ListKind.Friend, "Friends"},
                {ListKind.Moderator, "Moderators"},
                {ListKind.Interested, "Interested"},
                {ListKind.NotInterested, "NotInterested"},
                {ListKind.Ignored, "Ignored"}
            };

        #endregion

        #region Constructors and Destructors

        public ManageListsViewModel(
            IChatModel cm, IUnityContainer contain, IRegionManager regman, IEventAggregator eventagg,
            ICharacterManager manager)
            : base(contain, regman, eventagg, cm, manager)
        {
            Container.RegisterType<object, ManageListsTabView>(ManageListsTabView);

            genderSettings = new GenderSettingsModel();
            SearchSettings.ShowNotInterested = true;
            SearchSettings.ShowIgnored = true;

            SearchSettings.Updated += (s, e) =>
                {
                    OnPropertyChanged("SearchSettings");
                    UpdateBindings();
                };

            GenderSettings.Updated += (s, e) =>
                {
                    OnPropertyChanged("GenderSettings");
                    UpdateBindings();
                };

            ChatModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName.Equals("OnlineFriends", StringComparison.OrdinalIgnoreCase))
                        OnPropertyChanged("Friends");
                };

            Events.GetEvent<NewUpdateEvent>().Subscribe(
                args =>
                    {
                        var thisChannelUpdate = args as ChannelUpdateModel;
                        if (thisChannelUpdate != null
                            && thisChannelUpdate.Arguments is ChannelUpdateModel.ChannelTypeBannedListEventArgs)
                        {
                            OnPropertyChanged("HasBanned");
                            OnPropertyChanged("Banned");
                        }

                        var thisUpdate = args as CharacterUpdateModel;
                        if (thisUpdate == null)
                            return;

                        var name = thisUpdate.TargetCharacter.Name;

                        var joinLeaveArguments = thisUpdate.Arguments as CharacterUpdateModel.JoinLeaveEventArgs;
                        if (joinLeaveArguments != null)
                        {
                            if (manager.IsOnList(name, ListKind.Moderator, false))
                                OnPropertyChanged("Moderators");
                            if (manager.IsOnList(name, ListKind.Banned, false))
                                OnPropertyChanged("Banned");
                            return;
                        }

                        var signInOutArguments = thisUpdate.Arguments as CharacterUpdateModel.LoginStateChangedEventArgs;
                        if (signInOutArguments != null)
                        {
                            listKinds.Each(x =>
                                {
                                    if (manager.IsOnList(name, x.Key, false))
                                        OnPropertyChanged(x.Value);
                                });

                            return;
                        }

                        var thisArguments = thisUpdate.Arguments as CharacterUpdateModel.ListChangedEventArgs;
                        if (thisArguments == null)
                            return;

                        switch (thisArguments.ListArgument)
                        {
                            case CharacterUpdateModel.ListChangedEventArgs.ListType.Interested:
                                OnPropertyChanged("Interested");
                                OnPropertyChanged("NotInterested");
                                break;
                            case CharacterUpdateModel.ListChangedEventArgs.ListType.Ignored:
                                OnPropertyChanged("Ignored");
                                break;
                            case CharacterUpdateModel.ListChangedEventArgs.ListType.NotInterested:
                                OnPropertyChanged("NotInterested");
                                OnPropertyChanged("Interested");
                                break;
                            case CharacterUpdateModel.ListChangedEventArgs.ListType.Bookmarks:
                                OnPropertyChanged("Bookmarks");
                                break;
                            case CharacterUpdateModel.ListChangedEventArgs.ListType.Friends:
                                OnPropertyChanged("Friends");
                                break;
                        }
                    },
                true);

            cm.SelectedChannelChanged += (s, e) =>
                {
                    OnPropertyChanged("HasUsers");
                    OnPropertyChanged("Moderators");
                    OnPropertyChanged("HasBanned");
                    OnPropertyChanged("Banned");
                };
        }

        #endregion

        #region Public Properties

        public IEnumerable<ICharacter> Banned
        {
            get
            {
                var channel = ChatModel.CurrentChannel as GeneralChannelModel;
                if (HasUsers && channel != null)
                    return channel.CharacterManager.GetCharacters(ListKind.Banned, false).OrderBy(x => x.Name);

                return null;
            }
        }

        public IEnumerable<ICharacter> Bookmarks
        {
            get { return CharacterManager.GetCharacters(ListKind.Bookmark, !showOffline).OrderBy(x => x.Name); }
        }

        public IEnumerable<ICharacter> Friends
        {
            get { return CharacterManager.GetCharacters(ListKind.Friend, !showOffline).OrderBy(x => x.Name); }
        }

        public GenderSettingsModel GenderSettings
        {
            get { return genderSettings; }
        }

        public bool HasBanned
        {
            get
            {
                var channel = ChatModel.CurrentChannel as GeneralChannelModel;
                if (HasUsers && channel != null)
                    return channel.CharacterManager.GetNames(ListKind.Banned, false).Count > 0;

                return false;
            }
        }

        public IEnumerable<ICharacter> Ignored
        {
            get { return CharacterManager.GetCharacters(ListKind.Ignored).OrderBy(x => x.Name); }
        }

        public IEnumerable<ICharacter> Interested
        {
            get { return CharacterManager.GetCharacters(ListKind.Interested, !showOffline).OrderBy(x => x.Name); }
        }

        public IEnumerable<ICharacter> Moderators
        {
            get
            {
                var channel = ChatModel.CurrentChannel as GeneralChannelModel;
                if (HasUsers && channel != null)
                    return channel.CharacterManager.GetCharacters(ListKind.Moderator, !showOffline).OrderBy(x => x.Name);

                return null;
            }
        }

        public IEnumerable<ICharacter> NotInterested
        {
            get { return CharacterManager.GetCharacters(ListKind.NotInterested, !showOffline).OrderBy(x => x.Name); }
        }

        public bool ShowMods
        {
            get { return ChatModel.CurrentChannel is GeneralChannelModel; }
        }

        public bool ShowOffline
        {
            get { return showOffline; }

            set
            {
                showOffline = value;
                UpdateBindings();
            }
        }

        #endregion

        #region Methods

        private bool MeetsFilter(ICharacter character)
        {
            return character.MeetsFilters(
                GenderSettings, SearchSettings, CharacterManager, ChatModel.CurrentChannel as GeneralChannelModel);
        }

        private void UpdateBindings()
        {
            OnPropertyChanged("Friends");
            OnPropertyChanged("Bookmarks");
            OnPropertyChanged("Interested");
            OnPropertyChanged("NotInterested");
            OnPropertyChanged("Ignored");
            OnPropertyChanged("Moderators");
            OnPropertyChanged("Banned");
        }

        #endregion
    }
}