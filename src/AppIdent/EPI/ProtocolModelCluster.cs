//Copyright (c) 2017 Jan Pluskal
//
//Permission is hereby granted, free of charge, to any person
//obtaining a copy of this software and associated documentation
//files (the "Software"), to deal in the Software without
//restriction, including without limitation the rights to use,
//copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the
//Software is furnished to do so, subject to the following
//conditions:
//
//The above copyright notice and this permission notice shall be
//included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//OTHER DEALINGS IN THE SOFTWARE.



using System;
using System.Linq;
using AppIdent.Models;
using AppIdent.Statistics;
using Castle.Core.Internal;
using numl.Unsupervised;

namespace AppIdent.EPI
{
    public class ProtocolModelCluster : Cluster
    {
        /// <summary>Default constructor.</summary>
        public ProtocolModelCluster(Cluster cluster)
        {
            this.Id = cluster.Id;
            this.Center = cluster.Center;
            this.Members = cluster.Members.Cast<FeatureVector>().ToArray();

            this.Children = cluster.Children?.Select(child => new ProtocolModelCluster(child)).ToArray();
            this.FlattenChildren = this.Children?.SelectMany(child => child?.FlattenChildren).ToArray() ?? new[]
            {
                this
            };
            this.FlattenChildrenAppTags = this.FlattenChildren.SelectMany(protocolModelCluster => protocolModelCluster.FlattenChildrenAppTags ?? new[]
            {
                protocolModelCluster.ClusterAppTags
            }).ToArray();
        }

        public ApplicationProtocolClassificationStatistics ApplicationProtocolClassificationStatistics { get; set; }

        public new ProtocolModelCluster[] Children { get; set; }
        public ProtocolModelCluster[] FlattenChildren { get; set; }
        public string[] FlattenChildrenAppTags { get; set; }
        public new FeatureVector[] Members { get; set; }

        public string ClusterAppTags => string.Join(", ", this.Members.OfType<FeatureVector>().Select(m => m.Label));

        public void UpdateStatistics(ApplicationProtocolClassificationStatisticsMeter applicationProtocolClassificationStatisticsMeter)
        {
            if(!this.Children.IsNullOrEmpty())
            {
                foreach(var cluster in this.Children) //drill down
                {
                    cluster.UpdateStatistics(applicationProtocolClassificationStatisticsMeter);
                }

                this.ApplicationProtocolClassificationStatistics = this.RecalculateApplicationProtocolClassificationStatisticsForCluster();
            }
            else { this.ApplicationProtocolClassificationStatistics = applicationProtocolClassificationStatisticsMeter[this.Members.First().Label]; }
        }

        private ApplicationProtocolClassificationStatistics RecalculateApplicationProtocolClassificationStatisticsForCluster()
        {
            if(this.FlattenChildren == null) return this.ApplicationProtocolClassificationStatistics;

            var clusterAppProtoClassStats = new ApplicationProtocolClassificationStatistics(this);

            foreach(var children in this.FlattenChildren)
            {
                if(children.ApplicationProtocolClassificationStatistics == null)
                {
                    continue; //TODO check, why is sometines null
                }
                var childrenStats = children.ApplicationProtocolClassificationStatistics;

                //TP
                clusterAppProtoClassStats.TP += childrenStats.TP;

                //FP
                foreach(var fpStat in childrenStats.FPsStatistics)
                {
                    if(this.FlattenChildrenAppTags.Contains(fpStat.Key, StringComparer.InvariantCultureIgnoreCase)) clusterAppProtoClassStats.TP += fpStat.Value;
                    else { clusterAppProtoClassStats.AddFP(fpStat.Key); }
                }

                //FN
                foreach(var fnStat in childrenStats.FNsStatistics)
                {
                    if(this.FlattenChildrenAppTags.Contains(fnStat.Key, StringComparer.InvariantCultureIgnoreCase)) clusterAppProtoClassStats.TP += fnStat.Value;
                    else { clusterAppProtoClassStats.AddFN(fnStat.Key); }
                }
            }
            return clusterAppProtoClassStats;
        }
    }
}