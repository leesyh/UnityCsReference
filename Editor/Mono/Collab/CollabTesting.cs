// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Collaboration
{
    internal class CollabTesting
    {
        [Flags]
        public enum AsyncState
        {
            NotWaiting = 0,
            WaitForJobComplete,
            WaitForAssetUpdate,
        }

        private static IEnumerator<AsyncState> _enumerator = null;
        private static Action _runAfter = null;
        private static AsyncState _nextState = AsyncState.NotWaiting;

        public static Func<IEnumerable<AsyncState>> Tick
        {
            set { _enumerator = value().GetEnumerator(); }
        }

        public static Action AfterRun
        {
            set { _runAfter = value; }
        }

        public static bool IsRunning
        {
            get { return _enumerator != null; }
        }

        public static void OnCompleteJob()
        {
            if ((_nextState & AsyncState.WaitForJobComplete) == 0)
                return;

            _nextState &= ~AsyncState.WaitForJobComplete;
            if (_nextState == AsyncState.NotWaiting)
                Execute();
        }

        public static void OnAssetUpdate()
        {
            if ((_nextState & AsyncState.WaitForAssetUpdate) == 0)
                return;

            _nextState &= ~AsyncState.WaitForAssetUpdate;
            if (_nextState == AsyncState.NotWaiting)
                Execute();
        }

        public static void Execute()
        {
            if (_enumerator == null)
                return;

            if (Collab.instance.AnyJobRunning())
                return;

            try
            {
                if (!_enumerator.MoveNext())
                    End();
                else
                    _nextState = _enumerator.Current;
            }
            catch (Exception)
            {
                Debug.LogError("Something Went wrong with the test framework itself");
                throw;
            }
        }

        public static void End()
        {
            if (_enumerator != null)
            {
                _runAfter();
                _enumerator = null;
            }
        }
    }
}