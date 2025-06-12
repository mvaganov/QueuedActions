using System;

namespace QueuedActions {
	public class ActionEntry {
		protected string id;
		protected Action whatToDo;
		protected float waitTime;
		protected ActionState[] states;
		protected byte blockingLayers;
		protected bool started, finished;
		public string ID { get => id; set => id = value; }
		public Action WhatToDo { get => whatToDo; set => whatToDo = value; }
		public float WaitTime { get => waitTime; set => waitTime = value; }
		public byte LayerMask { get => blockingLayers; set => blockingLayers = value; }
		public ActionState[] States { get => states; set => states = value; }
		public bool UsesLayers => blockingLayers != 0;
		public bool IsOnLayer(int layer) { return (blockingLayers & (1 << layer)) != 0; }
		public bool IsStarted => started;
		public bool IsFinished => finished || IsFinishedCheck();
		public void Reset() { started = finished = false; }
		public bool SetBlocking(int layer, bool blocked) {
			if (layer >= 255 || layer < 0) return false;
			if (blocked) blockingLayers &= (byte)layer;
			else { blockingLayers &= (byte)~layer; }
			return true;
		}
		public void Start() {
			WhatToDo?.Invoke();
			started = true;
		}
		public bool IsFinishedCheck() {
			if (finished) return true;
			if (states == null) { return finished = true; }
			bool finishByDefault = true;
			for (int i = 0; i < states.Length; ++i) {
				ActionState state = states[i];
				if (state == null || (state.Check == null && state.Activate == null)) {
					continue;
				}
				finishByDefault = false;
				if (state.Check == null || state.Check.Invoke()) {
					state.Activate?.Invoke();
					return finished = true;
				}
			}
			return finished = finishByDefault;
		}
		public ActionState GetState(string name) {
			if (states == null) { return null; }
			for (int i = 0; i < states.Length; i++) {
				if (states[i].Name == name) {
					return states[i];
				}
			}
			return null;
		}
		public ActionState CreateState(string name) {
			if (states == null) { states = new ActionState[1]; } else { Array.Resize(ref states, states.Length + 1); }
			return states[states.Length - 1] = new ActionState(name);
		}
	}

	public class ActionState {
		public string Name;
		public Func<bool> Check;
		public Action Activate;
		public ActionState(string name, Func<bool> check, Action action) { Name = name; Check = check; Activate = action; }
		public ActionState(string name, Func<bool> check) { Name = name; Check = check; }
		public ActionState(string name, Action action) { Name = name; Activate = action; }
		public ActionState(string name) { Name = name; }
	}
}
