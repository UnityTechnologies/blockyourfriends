using System;
using System.Collections.Generic;

namespace BlockYourFriends.Multiplayer.Auth
{
    /// <summary>
    /// Represents some provider of credentials.
    /// Each provider will have its own identity needs, so we'll allow each to define whatever parameters it needs.
    /// Anything that accesses the contents should know what it's looking for.
    /// </summary>
    public class SubIdentity : Observed<SubIdentity>
    {
        protected Dictionary<string, string> contents = new Dictionary<string, string>();

        public string GetContent(string key)
        {
            if (!contents.ContainsKey(key))
                contents.Add(key, null); // Not alerting observers via OnChanged until the value is actually present (especially since this could be called by an observer, which would be cyclical).
            return contents[key];
        }

        public void SetContent(string key, string value)
        {
            if (!contents.ContainsKey(key))
                contents.Add(key, value);
            else
                contents[key] = value;
            OnChanged(this);
        }

        public override void CopyObserved(SubIdentity oldObserved)
        {
            contents = oldObserved.contents;
        }
    }

    public enum IIdentityType { Local = 0, Auth }

    public interface IIdentity : IProvidable<IIdentity>
    {
        SubIdentity GetSubIdentity(IIdentityType identityType);
    }

    public class IdentityNoop : IIdentity
    {
        public SubIdentity GetSubIdentity(IIdentityType identityType) { return null; }
        public void OnReProvided(IIdentity other) { }
    }

    /// <summary>
    /// Our internal representation of the local player's credentials, wrapping the data required for interfacing with the identities of that player in the services.
    /// (In use here, it just wraps Auth, but it can be used to combine multiple sets of credentials into one concept of a player.)
    /// </summary>
    public class Identity : IIdentity, IDisposable
    {
        private Dictionary<IIdentityType, SubIdentity> subIdentities = new Dictionary<IIdentityType, SubIdentity>();

        public Identity(Action callbackOnAuthLogin)
        {
            subIdentities.Add(IIdentityType.Local, new SubIdentity());
            subIdentities.Add(IIdentityType.Auth, new SubIdentity_Authentication(callbackOnAuthLogin));
        }

        public SubIdentity GetSubIdentity(IIdentityType identityType)
        {
            return subIdentities[identityType];
        }

        public void OnReProvided(IIdentity prev)
        {
            if (prev is Identity)
            {
                Identity prevIdentity = prev as Identity;
                foreach (var entry in prevIdentity.subIdentities)
                    subIdentities.Add(entry.Key, entry.Value);
            }
        }

        public void Dispose()
        {
            foreach (var sub in subIdentities)
                if (sub.Value is IDisposable)
                    (sub.Value as IDisposable).Dispose();
        }
    }
}
