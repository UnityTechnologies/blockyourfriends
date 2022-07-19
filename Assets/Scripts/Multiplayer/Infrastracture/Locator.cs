﻿using System;
using System.Collections.Generic;
using BlockYourFriends.Multiplayer.Auth;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Anything which provides itself to a Locator can then be globally accessed. This should be a single access point for things that *want* to be singleton (that is,
    /// when they want to be available for use by arbitrary, unknown clients) but might not always be available or might need alternate flavors for tests, logging, etc.
    /// (See http://gameprogrammingpatterns.com/service-locator.html to learn more.)
    /// </summary>
    public class Locator : LocatorBase
    {
        private static Locator instance;

        public static Locator Get
        {
            get
            {
                if (instance == null)
                    instance = new Locator();
                return instance;
            }
        }

        protected override void FinishConstruction()
        {
            instance = this;
        }
    }

    /// <summary>
    /// Allows Located services to transfer data to their replacements if needed.
    /// </summary>
    /// <typeparam name="T">The base interface type you want to Provide.</typeparam>
    public interface IProvidable<T>
    {
        void OnReProvided(T previousProvider);
    }

    /// <summary>
    /// Base Locator behavior, without static access.
    /// </summary>
    public class LocatorBase
    {
        private Dictionary<Type, object> provided = new Dictionary<Type, object>();

        /// <summary>
        /// On construction, we can prepare default implementations of any services we expect to be required. This way, if for some reason the actual implementations
        /// are never Provided (e.g. for tests), nothing will break.
        /// </summary>
        public LocatorBase()
        {
            Provide(new Messenger());
            Provide(new UpdateSlowNoop());
            Provide(new IdentityNoop());

            FinishConstruction();
        }

        protected virtual void FinishConstruction() { }

        /// <summary>
        /// Call this to indicate that something is available for global access.
        /// </summary>
        private void ProvideAny<T>(T instance) where T : IProvidable<T>
        {
            Type type = typeof(T);
            if (provided.ContainsKey(type))
            {
                var previousProvision = (T)provided[type];
                instance.OnReProvided(previousProvision);
                provided.Remove(type);
            }

            provided.Add(type, instance);
        }

        /// <summary>
        /// If a T has previously been Provided, this will retrieve it. Else, null is returned.
        /// </summary>
        private T Locate<T>() where T : class
        {
            Type type = typeof(T);
            if (!provided.ContainsKey(type))
                return null;
            return provided[type] as T;
        }

        // To limit global access to only components that should have it, and to reduce programmer error, we'll declare explicit flavors of Provide and getters for them.
        public IMessenger Messenger => Locate<IMessenger>();
        public void Provide(IMessenger messenger) { ProvideAny(messenger); }

        public IUpdateSlow UpdateSlow => Locate<IUpdateSlow>();
        public void Provide(IUpdateSlow updateSlow) { ProvideAny(updateSlow); }

        public IIdentity Identity => Locate<IIdentity>();
        public void Provide(IIdentity identity) { ProvideAny(identity); }

        // As you add more Provided types, be sure their default implementations are included in the constructor.
    }
}