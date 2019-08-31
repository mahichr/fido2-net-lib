﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fido2NetLib.Objects;

namespace Fido2NetLib.Development
{
    public class DevelopmentInMemoryStore
    {
        ConcurrentDictionary<string, Fido2User> storedUsers = new ConcurrentDictionary<string, Fido2User>();

        public Fido2User GetOrAddUser(string username, Func<Fido2User> addCallback)
        {
            return storedUsers.GetOrAdd(username, addCallback());
        }

        public Fido2User GetUser(string username)
        {
            storedUsers.TryGetValue(username, out var user);
            return user;
        }


        List<StoredCredential> storedCredentials = new List<StoredCredential>();

        public List<StoredCredential> GetCredentialsByUser(Fido2User user)
        {
            return storedCredentials.Where(c => c.UserId.SequenceEqual(user.Id)).ToList();
        }

        public StoredCredential GetCredentialById(byte[] id)
        {
            return storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(id)).FirstOrDefault();
        }

        public Task<List<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
        {
            return Task.FromResult(storedCredentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList());
        }

        public void UpdateCounter(byte[] credentialId, uint counter)
        {
            var cred = storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();
            cred.SignatureCounter = counter;
        }

        public void AddCredentialToUser(Fido2User user, StoredCredential credential)
        {
            credential.UserId = user.Id;
            storedCredentials.Add(credential);
        }

        public Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId)
        {
            // our in-mem storage does not allow storing multiple users for a given credentialId. Yours shouldn't either.
            var cred = storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();

            if (cred == null) return Task.FromResult(new List<Fido2User>());

            return Task.FromResult(storedUsers.Where(u => u.Value.Id.SequenceEqual(cred.UserId)).Select(u => u.Value).ToList());
        }
    }

    public class StoredCredential
    {
        public byte[] UserId { get; set; }
        public PublicKeyCredentialDescriptor Descriptor { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
    }
}