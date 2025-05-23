// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Diagnostics
{
    public sealed class ActivitySource : IDisposable
    {
        private static readonly SynchronizedList<ActivitySource> s_activeSources = new SynchronizedList<ActivitySource>();
        private static readonly SynchronizedList<ActivityListener> s_allListeners = new SynchronizedList<ActivityListener>();
        private SynchronizedList<ActivityListener>? _listeners;

        /// <summary>
        /// Construct an ActivitySource object with the input name
        /// </summary>
        /// <param name="name">The name of the ActivitySource object</param>
        public ActivitySource(string name) : this(name, version: "", tags: null, telemetrySchemaUrl: null) {}

        /// <summary>
        /// Construct an ActivitySource object with the input name
        /// </summary>
        /// <param name="name">The name of the ActivitySource object</param>
        /// <param name="version">The version of the component publishing the tracing info.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ActivitySource(string name, string? version = "") : this(name, version, tags: null, telemetrySchemaUrl: null) {}

        /// <summary>
        /// Construct an ActivitySource object with the input name
        /// </summary>
        /// <param name="name">The name of the ActivitySource object</param>
        /// <param name="version">The version of the component publishing the tracing info.</param>
        /// <param name="tags">The optional ActivitySource tags.</param>
        public ActivitySource(string name, string? version = "", IEnumerable<KeyValuePair<string, object?>>? tags = default) : this(name, version, tags, telemetrySchemaUrl: null) {}

        /// <summary>
        /// Initialize a new instance of the ActivitySource object using the <see cref="ActivitySourceOptions" />.
        /// </summary>
        /// <param name="options">The <see cref="ActivitySourceOptions" /> object to use for initializing the ActivitySource object.</param>
        public ActivitySource(ActivitySourceOptions options) : this((options ?? throw new ArgumentNullException(nameof(options))).Name, options.Version, options.Tags, options.TelemetrySchemaUrl) {}

        private ActivitySource(string name, string? version, IEnumerable<KeyValuePair<string, object?>>? tags, string? telemetrySchemaUrl)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version;
            TelemetrySchemaUrl = telemetrySchemaUrl;

            // Sorting the tags to make sure the tags are always in the same order.
            // Sorting can help in comparing the tags used for any scenario.
            if (tags is not null)
            {
                var tagList = new List<KeyValuePair<string, object?>>(tags);
                tagList.Sort((left, right) => string.Compare(left.Key, right.Key, StringComparison.Ordinal));
                Tags = tagList.AsReadOnly();
            }

            s_activeSources.Add(this);

            s_allListeners.EnumWithAction((listener, source) =>
            {
                Func<ActivitySource, bool>? shouldListenTo = listener.ShouldListenTo;
                if (shouldListenTo != null)
                {
                    var activitySource = (ActivitySource)source;
                    if (shouldListenTo(activitySource))
                    {
                        activitySource.AddListener(listener);
                    }
                }
            }, this);

            GC.KeepAlive(DiagnosticSourceEventSource.Log);
        }

        /// <summary>
        /// Returns the ActivitySource name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Returns the ActivitySource version.
        /// </summary>
        public string? Version { get; }

        /// <summary>
        /// Returns the tags associated with the ActivitySource.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object?>>? Tags { get; }

        /// <summary>
        /// Returns the telemetry schema URL associated with the ActivitySource.
        /// </summary>
        public string? TelemetrySchemaUrl { get; }

        /// <summary>
        /// Check if there is any listeners for this ActivitySource.
        /// This property can be helpful to tell if there is no listener, then no need to create Activity object
        /// and avoid creating the objects needed to create Activity (e.g. ActivityContext)
        /// Example of that is http scenario which can avoid reading the context data from the wire.
        /// </summary>
        public bool HasListeners()
        {
            SynchronizedList<ActivityListener>? listeners = _listeners;
            return listeners != null && listeners.Count > 0;
        }

        /// <summary>
        /// Creates a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
        /// </summary>
        /// <param name="name">The operation name of the Activity</param>
        /// <param name="kind">The <see cref="ActivityKind"/></param>
        /// <returns>The created <see cref="Activity"/> object or null if there is no any event listener.</returns>
        /// <remarks>
        /// If the Activity object is created, it will not start automatically. Callers need to call <see cref="Activity.Start()"/> to start it.
        /// </remarks>
        public Activity? CreateActivity(string name, ActivityKind kind)
            => CreateActivity(name, kind, default, null, null, null, default, startIt: false);

        /// <summary>
        /// Creates a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
        /// If the Activity object is created, it will not automatically start. Callers will need to call <see cref="Activity.Start()"/> to start it.
        /// </summary>
        /// <param name="name">The operation name of the Activity.</param>
        /// <param name="kind">The <see cref="ActivityKind"/></param>
        /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
        /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
        /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
        /// <param name="idFormat">The default Id format to use.</param>
        /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
        /// <remarks>
        /// If the Activity object is created, it will not start automatically. Callers need to call <see cref="Activity.Start()"/> to start it.
        /// </remarks>
        public Activity? CreateActivity(string name, ActivityKind kind, ActivityContext parentContext, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, ActivityIdFormat idFormat = ActivityIdFormat.Unknown)
            => CreateActivity(name, kind, parentContext, null, tags, links, default, startIt: false, idFormat);

        /// <summary>
        /// Creates a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
        /// </summary>
        /// <param name="name">The operation name of the Activity.</param>
        /// <param name="kind">The <see cref="ActivityKind"/></param>
        /// <param name="parentId">The parent Id to initialize the created Activity object with.</param>
        /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
        /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
        /// <param name="idFormat">The default Id format to use.</param>
        /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
        /// <remarks>
        /// If the Activity object is created, it will not start automatically. Callers need to call <see cref="Activity.Start()"/> to start it.
        /// </remarks>
        public Activity? CreateActivity(string name, ActivityKind kind, string? parentId, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, ActivityIdFormat idFormat = ActivityIdFormat.Unknown)
            => CreateActivity(name, kind, default, parentId, tags, links, default, startIt: false, idFormat);

        /// <summary>
        /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
        /// </summary>
        /// <param name="name">The operation name of the Activity</param>
        /// <param name="kind">The <see cref="ActivityKind"/></param>
        /// <returns>The created <see cref="Activity"/> object or null if there is no any event listener.</returns>
        public Activity? StartActivity([CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Internal)
            => CreateActivity(name, kind, default, null, null, null, default);

        /// <summary>
        /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.
        /// </summary>
        /// <param name="name">The operation name of the Activity.</param>
        /// <param name="kind">The <see cref="ActivityKind"/></param>
        /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
        /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
        /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
        /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
        /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
        public Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default)
            => CreateActivity(name, kind, parentContext, null, tags, links, startTime);

        /// <summary>
        /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.
        /// </summary>
        /// <param name="name">The operation name of the Activity.</param>
        /// <param name="kind">The <see cref="ActivityKind"/></param>
        /// <param name="parentId">The parent Id to initialize the created Activity object with.</param>
        /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
        /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
        /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
        /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
        public Activity? StartActivity(string name, ActivityKind kind, string? parentId, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default)
            => CreateActivity(name, kind, default, parentId, tags, links, startTime);

        /// <summary>
        /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.
        /// </summary>
        /// <param name="kind">The <see cref="ActivityKind"/></param>
        /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
        /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
        /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
        /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
        /// <param name="name">The operation name of the Activity.</param>
        /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
        public Activity? StartActivity(ActivityKind kind, ActivityContext parentContext = default, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default, [CallerMemberName] string name = "")
            => CreateActivity(name, kind, parentContext, null, tags, links, startTime);

        private Activity? CreateActivity(string name, ActivityKind kind, ActivityContext context, string? parentId, IEnumerable<KeyValuePair<string, object?>>? tags,
                                            IEnumerable<ActivityLink>? links, DateTimeOffset startTime, bool startIt = true, ActivityIdFormat idFormat = ActivityIdFormat.Unknown)
        {
            // _listeners can get assigned to null in Dispose.
            SynchronizedList<ActivityListener>? listeners = _listeners;
            if (listeners == null || listeners.Count == 0)
            {
                return null;
            }

            Activity? activity = null;
            ActivityTagsCollection? samplerTags;
            string? traceState;

            ActivitySamplingResult samplingResult = ActivitySamplingResult.None;

            if (parentId != null)
            {
                ActivityCreationOptions<string> aco = default;
                ActivityCreationOptions<ActivityContext> acoContext = default;

                aco = new ActivityCreationOptions<string>(this, name, parentId, kind, tags, links, idFormat);
                if (aco.IdFormat == ActivityIdFormat.W3C)
                {
                    // acoContext is used only in the Sample calls which called only when we have W3C Id format.
                    acoContext = new ActivityCreationOptions<ActivityContext>(this, name, aco.GetContext(), kind, tags, links, ActivityIdFormat.W3C);
                }

                listeners.EnumWithFunc((ActivityListener listener, ref ActivityCreationOptions<string> data, ref ActivitySamplingResult result, ref ActivityCreationOptions<ActivityContext> dataWithContext) => {
                    SampleActivity<string>? sampleUsingParentId = listener.SampleUsingParentId;
                    if (sampleUsingParentId != null)
                    {
                        ActivitySamplingResult sr = sampleUsingParentId(ref data);
                        dataWithContext.SetTraceState(data.TraceState); // Keep the trace state in sync between data and dataWithContext

                        if (sr > result)
                        {
                            result = sr;
                        }
                    }
                    else if (data.IdFormat == ActivityIdFormat.W3C)
                    {
                        // In case we have a parent Id and the listener not providing the SampleUsingParentId, we'll try to find out if the following conditions are true:
                        //   - The listener is providing the Sample callback
                        //   - Can convert the parent Id to a Context. ActivityCreationOptions.TraceId != default means parent id converted to a valid context.
                        // Then we can call the listener Sample callback with the constructed context.
                        SampleActivity<ActivityContext>? sample = listener.Sample;
                        if (sample != null)
                        {
                            ActivitySamplingResult sr = sample(ref dataWithContext);
                            data.SetTraceState(dataWithContext.TraceState); // Keep the trace state in sync between data and dataWithContext

                            if (sr > result)
                            {
                                result = sr;
                            }
                        }
                    }
                }, ref aco, ref samplingResult, ref acoContext);

                if (context == default)
                {
                    if (aco.GetContext() != default)
                    {
                        context = aco.GetContext();
                        parentId = null;
                    }
                    else if (acoContext.GetContext() != default)
                    {
                        context = acoContext.GetContext();
                        parentId = null;
                    }
                }

                samplerTags = aco.GetSamplingTags();
                ActivityTagsCollection? atc = acoContext.GetSamplingTags();
                if (atc != null)
                {
                    if (samplerTags == null)
                    {
                        samplerTags = atc;
                    }
                    else
                    {
                        foreach (KeyValuePair<string, object?> tag in atc)
                        {
                            samplerTags.Add(tag);
                        }
                    }
                }

                idFormat = aco.IdFormat;
                traceState = aco.TraceState;
            }
            else
            {
                bool useCurrentActivityContext = context == default && Activity.Current != null;
                var aco = new ActivityCreationOptions<ActivityContext>(this, name, useCurrentActivityContext ? Activity.Current!.Context : context, kind, tags, links, idFormat);
                listeners.EnumWithFunc((ActivityListener listener, ref ActivityCreationOptions<ActivityContext> data, ref ActivitySamplingResult result, ref ActivityCreationOptions<ActivityContext> unused) => {
                    SampleActivity<ActivityContext>? sample = listener.Sample;
                    if (sample != null)
                    {
                        ActivitySamplingResult dr = sample(ref data);
                        if (dr > result)
                        {
                            result = dr;
                        }
                    }
                }, ref aco, ref samplingResult, ref aco);

                if (!useCurrentActivityContext)
                {
                    // We use the context stored inside ActivityCreationOptions as it is possible the trace id get automatically generated during the sampling.
                    // We don't use the context stored inside ActivityCreationOptions only in case if we used Activity.Current context, the reason is we need to
                    // create the new child activity with Parent set to Activity.Current.
                    context = aco.GetContext();
                }

                samplerTags = aco.GetSamplingTags();
                idFormat = aco.IdFormat;
                traceState = aco.TraceState;
            }

            if (samplingResult != ActivitySamplingResult.None)
            {
                activity = Activity.Create(this, name, kind, parentId, context, tags, links, startTime, samplerTags, samplingResult, startIt, idFormat, traceState);
            }

            return activity;
        }

        /// <summary>
        /// Dispose the ActivitySource object and remove the current instance from the global list. empty the listeners list too.
        /// </summary>
        public void Dispose()
        {
            _listeners = null;
            s_activeSources.Remove(this);
        }

        /// <summary>
        /// Add a listener to the <see cref="Activity"/> starting and stopping events.
        /// </summary>
        /// <param name="listener"> The <see cref="ActivityListener"/> object to use for listening to the <see cref="Activity"/> events.</param>
        public static void AddActivityListener(ActivityListener listener)
        {
            ArgumentNullException.ThrowIfNull(listener);

            if (s_allListeners.AddIfNotExist(listener))
            {
                s_activeSources.EnumWithAction((source, obj) => {
                    var shouldListenTo = ((ActivityListener)obj).ShouldListenTo;
                    if (shouldListenTo != null && shouldListenTo(source))
                    {
                        source.AddListener((ActivityListener)obj);
                    }
                }, listener);
            }
        }

        internal delegate void Function<T, TParent>(T item, ref ActivityCreationOptions<TParent> data, ref ActivitySamplingResult samplingResult, ref ActivityCreationOptions<ActivityContext> dataWithContext);

        internal void AddListener(ActivityListener listener)
        {
            if (_listeners == null)
            {
                Interlocked.CompareExchange(ref _listeners, new SynchronizedList<ActivityListener>(), null);
            }

            _listeners.AddIfNotExist(listener);
        }

        internal static void DetachListener(ActivityListener listener)
        {
            s_allListeners.Remove(listener);
            s_activeSources.EnumWithAction((source, obj) => source._listeners?.Remove((ActivityListener) obj), listener);
        }

        internal void NotifyActivityStart(Activity activity)
        {
            Debug.Assert(activity != null);

            // _listeners can get assigned to null in Dispose.
            SynchronizedList<ActivityListener>? listeners = _listeners;
            if (listeners != null && listeners.Count > 0)
            {
                listeners.EnumWithAction((listener, obj) => listener.ActivityStarted?.Invoke((Activity)obj), activity);
            }
        }

        internal void NotifyActivityStop(Activity activity)
        {
            Debug.Assert(activity != null);

            // _listeners can get assigned to null in Dispose.
            SynchronizedList<ActivityListener>? listeners = _listeners;
            if (listeners != null && listeners.Count > 0)
            {
                listeners.EnumWithAction((listener, obj) => listener.ActivityStopped?.Invoke((Activity)obj), activity);
            }
        }

        internal void NotifyActivityAddException(Activity activity, Exception exception, ref TagList tags)
        {
            Debug.Assert(activity != null);

            // _listeners can get assigned to null in Dispose.
            SynchronizedList<ActivityListener>? listeners = _listeners;
            if (listeners != null && listeners.Count > 0)
            {
                listeners.EnumWithExceptionNotification(activity, exception, ref tags);
            }
        }
    }

    // This class uses a copy-on-write design to ensure thread safety all operations are thread safe.
    // However, it is possible for read-only operations to see stale versions of the item while a change
    // is occurring.
    internal sealed class SynchronizedList<T>
    {
        private readonly object _writeLock;
        // This array must not be mutated directly. To mutate, obtain the lock, copy the array and then replace it with the new array.
        private T[] _volatileArray;
        public SynchronizedList()
        {
            _volatileArray = [];
            _writeLock = new();
        }

        public void Add(T item)
        {
            lock (_writeLock)
            {
                T[] newArray = new T[_volatileArray.Length + 1];

                Array.Copy(_volatileArray, newArray, _volatileArray.Length);// copy existing items
                newArray[_volatileArray.Length] = item;// copy new item

                _volatileArray = newArray;
            }
        }

        public bool AddIfNotExist(T item)
        {
            lock (_writeLock)
            {
                int index = Array.IndexOf(_volatileArray, item);

                if (index >= 0)
                {
                    return false;
                }

                T[] newArray = new T[_volatileArray.Length + 1];

                Array.Copy(_volatileArray, newArray, _volatileArray.Length);// copy existing items
                newArray[_volatileArray.Length] = item;// copy new item

                _volatileArray = newArray;

                return true;
            }
        }

        public bool Remove(T item)
        {
            lock (_writeLock)
            {
                int index = Array.IndexOf(_volatileArray, item);

                if (index < 0)
                {
                    return false;
                }

                T[] newArray = new T[_volatileArray.Length - 1];

                Array.Copy(_volatileArray, newArray, index);// copy existing items before index

                Array.Copy(
                    _volatileArray, index + 1, // position after the index, skipping it
                    newArray, index, _volatileArray.Length - index - 1// remaining items accounting for removed item
                );

                _volatileArray = newArray;
                return true;
            }
        }

        public int Count => _volatileArray.Length;

        public void EnumWithFunc<TParent>(ActivitySource.Function<T, TParent> func, ref ActivityCreationOptions<TParent> data, ref ActivitySamplingResult samplingResult, ref ActivityCreationOptions<ActivityContext> dataWithContext)
        {
            foreach (T item in _volatileArray)
            {
                func(item, ref data, ref samplingResult, ref dataWithContext);
            }
        }

        public void EnumWithAction(Action<T, object> action, object arg)
        {
            foreach (T item in _volatileArray)
            {
                action(item, arg);
            }
        }

        public void EnumWithExceptionNotification(Activity activity, Exception exception, ref TagList tags)
        {
            if (typeof(T) != typeof(ActivityListener))
            {
                return;
            }

            foreach (T item in _volatileArray)
            {
                (item as ActivityListener)!.ExceptionRecorder?.Invoke(activity, exception, ref tags);
            }
        }
    }
}
