﻿using System;
using System.Management.Instrumentation;
using System.Text;
using ZooKeeperNet;

namespace SolrNet.Cloud {
    public class SolrCluster : ISolrCluster, IWatcher {
        public SolrCluster(ISolrClusterBalancer clusterBalancer, int maxAttempts, ISolrOperationsProvider operationsProvider, string zooKeeperConnection) {
            if (maxAttempts < 1)
                throw new ArgumentOutOfRangeException("maxAttempts");
            if (clusterBalancer == null)
                throw new ArgumentNullException("clusterBalancer");
            if (operationsProvider == null)
                throw new ArgumentNullException("operationsProvider");
            if (string.IsNullOrEmpty(zooKeeperConnection))
                throw new ArgumentNullException("zooKeeperConnection");
            this.maxAttempts = maxAttempts;
            this.clusterBalancer = clusterBalancer;
            this.operationsProvider = operationsProvider;
            this.zooKeeperConnection = zooKeeperConnection;
            exceptionHandlers = new SolrClusterExceptionHandlers(this);
            syncLock = new object();
        }

        private readonly ISolrClusterBalancer clusterBalancer;

        private bool isDisposed;

        private readonly SolrClusterExceptionHandlers exceptionHandlers;

        private bool isInitialized;

        private readonly int maxAttempts;

        private readonly ISolrOperationsProvider operationsProvider;

        private readonly object syncLock;

        private IZooKeeper zooKeeper;

        private readonly string zooKeeperConnection;

        public ISolrClusterCores Cores { get; protected set; }

        public void Dispose() {
            lock (syncLock)
                if (!isDisposed) {

                    isDisposed = true;
                }
        }

        public ISolrOperations<T> GetOperations<T>(string coreName, int? routingHash = null) {
            if (!isInitialized)
                throw new InvalidOperationException("This object was not initialized yet.");
            var core = Cores[coreName];
            if (core == null)
                throw new InstanceNotFoundException("No appropriate core was found.");
            var shard = core.Shards[routingHash];
            if (shard == null)
                throw new InstanceNotFoundException("No appropriate shard was found.");
            return new SolrClusterOperations<T>(clusterBalancer, exceptionHandlers, maxAttempts, operationsProvider, shard.Replicas);
        }

        public ISolrOperations<T> GetOperations<T>(string coreName = null, string shardName = null) {
            if (!isInitialized)
                throw new InvalidOperationException("This object was not initialized yet.");
            var core = Cores[coreName];
            if (core == null)
                throw new InstanceNotFoundException("No appropriate core was found.");
            var shard = core.Shards[shardName];
            if (shard == null)
                throw new InstanceNotFoundException("No appropriate shard was found.");
            return new SolrClusterOperations<T>(clusterBalancer, exceptionHandlers, maxAttempts, operationsProvider, shard.Replicas);
        }

        public bool Initialize() {
            lock (syncLock)
                if (!isInitialized) {
                    Update();
                    isInitialized = Update();
                }
            return isInitialized;
        }

        void IWatcher.Process(WatchedEvent @event) {
            if (@event.Type == EventType.NodeDataChanged)
                Update();
        }

        private bool Update() {
            try {
                zooKeeper = new ZooKeeper(zooKeeperConnection, TimeSpan.FromSeconds(10), this);
                Cores = SolrClusterCores.Create(
                    this, 
                    Encoding.Default.GetString(
                        zooKeeper.GetData("/clusterstate.json", true, null)));
                return true;
            } catch (Exception exception) {
                exceptionHandlers.Handle(exception);
            }
            return false;
        }

        public event EventHandler<SolrClusterExceptionEventArgs> Exception {
            add { exceptionHandlers.Add(value); }
            remove { exceptionHandlers.Remove(value); }
        }
    }
}