//USING
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using C5;

class InterfaceBuffer : StatisticsGenerator
{
//DEFINITIONS
    class Entry
    {
        public Bundle Bundle;
        public TimerEntry Timeout;
        public double WhenAdded;
    }
//CONSTRUCTION
    public InterfaceBuffer(NetworkInterface owner)
        : base(owner, InterfaceBufferTag)
    {
    }
//INTERFACE
    public Bundle GetNext( out double whenAdded)
    {
        if (bundles.IsEmpty)
        {
            ++bufferEmpty;
            whenAdded = 0;
            return null;
        }

        Entry entry = bundles.Dequeue();
        Timer.Cancel(entry.Timeout);
        size -= entry.Bundle.Size;
        passedData += entry.Bundle.Size;
        whenAdded = entry.WhenAdded;
        return entry.Bundle;
    }
    public void Append(Bundle bundle)
    {
        receivedData += bundle.Size;

        if (size + bundle.Size > Configuration.Network.Node.Buffer.DefaultBufferSize)
        {
            ++discardedBundles;
            discardedData += bundle.Size;
            return;
        }
        Entry entry = new Entry();
        entry.Bundle = bundle;
        entry.Timeout = Timer.Schedule(bundle.LifeTimeEnd, onBundleLifeTimeEnd, entry);
        entry.WhenAdded = Timer.CurrentTime;
        size += bundle.Size;
        bundles.Enqueue(entry);
    }
    public void OnLinkBroken()
    {
    }
    public void Clear()
    {
        discardedBundles += (uint)bundles.Count;
        discardedOnBreak += (uint)bundles.Count;
        while(bundles.Count>0)
        {
            Timer.Cancel(bundles.Dequeue().Timeout);
        }
        discardedData += size;
        size = 0;
    }

    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        statistics.Add(sizeId, size);
        statistics.Add(bundlesCountId, bundles.Count);
        statistics.Add(discardedBundlesId, discardedBundles);
        statistics.Add(discardedDataId, discardedData);
        statistics.Add(receivedDataId, receivedData);
        statistics.Add(passedDataId, passedData);
        statistics.Add(timeoutedBundlesId, timeoutedBundles);
        statistics.Add(bufferEmptyId, bufferEmpty);
        statistics.Add(bundlesDiscardedOnBreakId, discardedOnBreak);
        return statistics;
    }
//HELPERS
    void onBundleLifeTimeEnd(TimerEntry timeout)
    {
        Entry entry = (Entry) timeout.UserData;
        bool removed = bundles.Remove(entry);
        Debug.Assert(removed);
        size -= entry.Bundle.Size;
        discardedData += entry.Bundle.Size;
        ++discardedBundles;
        ++timeoutedBundles;
    }
//DATA
    C5.LinkedList<Entry> bundles = new C5.LinkedList<Entry>();
    C5.HashSet<Bundle> acquired = new HashSet<Bundle>();
    ulong size;
    uint discardedBundles;
    uint timeoutedBundles;
    uint discardedOnBreak;
    uint bufferEmpty;
    ulong receivedData;
    ulong passedData;
    ulong discardedData;
//CONSTANTS
    public const string InterfaceBufferTag = "InterfaceBuffer";
    const string sizeId = "Size";
    const string bundlesCountId = "BundlesCount";
    const string discardedBundlesId = "DiscardedBundles";
    const string receivedDataId = "ReceivedData";
    const string passedDataId = "PassedData";
    const string discardedDataId = "DiscardedData";
    const string timeoutedBundlesId = "TimeoutedBundles";
    const string bufferEmptyId = "BufferEmptyCount";
    const string bundlesDiscardedOnBreakId = "BundlesDiscardedOnBreak";
}