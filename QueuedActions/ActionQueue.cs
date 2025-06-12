using System;
using System.Collections.Generic;
using System.Text;

namespace QueuedActions {
	public class ActionQueue {
		protected List<ActionEntry> list = new List<ActionEntry>();
		protected int index;
		protected float waitTime;
		protected ActionEntry currentEntry;
		public int Index => index;
		public float WaitTime => waitTime;
		public int Count => list.Count;
		public ActionEntry this[int index] {
			get { return list[index]; }
			set { list[index] = value; }
		}
		public ActionEntry Current => index < list.Count ? list[index] : null;
		public void ResetCurrentAction() { currentEntry?.Reset(); }
		public void ResetAllActions() { list.ForEach(a => a.Reset()); }
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append("@").Append(index);
			if (index >= list.Count) { return sb.ToString(); }
			sb.Append(" [");
			for (int i = index; i < list.Count; ++i) {
				if (i > index) { sb.Append("], ["); }
				sb.Append(list[i].ID);
			}
			sb.Append("]");
			return sb.ToString();
		}
		public void Add(ActionEntry e) {
			list.Add(e);
		}
		public void RemoveAt(int lastAction) {
			list.RemoveAt(lastAction);
		}

		public void InsertNext(ActionEntry entry) {
			list.Insert(index+1, entry);
		}
		public void Update(float deltaTime) {
			EnsureCurrentProcess();
			if (!IsCurrentProcessStarted()) {
				StartCurrentProcess();
			}
			if (IsCurrentProcessFinished(deltaTime)) {
				StartNextProcess();
			}
		}
		private void EnsureCurrentProcess() {
			if (index >= list.Count) {
				currentEntry = null;
			} else if (currentEntry == null && index < list.Count) {
				currentEntry = list[index];
			}
		}
		public bool IsCurrentProcessStarted() => currentEntry == null || currentEntry.IsStarted;
		public bool IsCurrentProcessFinished(float deltaTime) {
			if (waitTime > 0) {
				waitTime -= deltaTime;
				if (waitTime > 0) {
					return false;
				}
				waitTime = 0;
			}
			return currentEntry == null || currentEntry.IsFinished;
		}
		public bool StartNextProcess() {
			++index;
			return StartCurrentProcess();
		}
		public bool StartCurrentProcess() {
			if (index >= list.Count) {
				return false;
			}
			currentEntry = list[index];
			currentEntry.Start();
			float waitTime = currentEntry.WaitTime;
			this.waitTime = waitTime > 0 ? waitTime : 0;
			return true;
		}
		public void UpdateSelfAndDistribute(float deltaTime, List<ActionQueue> layers) {
			if (waitTime > 0) {
				Update(deltaTime);
				return;
			}
			while (index < list.Count) {
				currentEntry = list[index];
				if (currentEntry.IsFinished) {
					++index;
					continue;
				}
				if (currentEntry.UsesLayers) {
					AddToLayers(currentEntry, layers);
					list.RemoveAt(index);
				} else {
					if (!IsCurrentProcessStarted()) {
						StartCurrentProcess();
						if (!IsCurrentProcessFinished(0)) {
							return;
						}
						index++;
					} else if (IsCurrentProcessFinished(deltaTime)) {
						StartNextProcess();
						if (!IsCurrentProcessFinished(0)) {
							return;
						}
					} else {
						return;
					}
				}
			}
			void AddToLayers(ActionEntry entry, List<ActionQueue> layers) {
				List<int> layersToAddTo = new List<int>();
				int maxLayer = -1;
				for (int i = 0; i < sizeof(byte) * 8; i++) {
					if ((entry.LayerMask & (1 << i)) != 0) {
						layersToAddTo.Add(i);
						maxLayer = i;
					}
				}
				while (layers.Count <= maxLayer) {
					layers.Add(null);
				}
				for (int i = 0; i < layers.Count; ++i) {
					if (entry.IsOnLayer(i)) {
						if (layers[i] == null) {
							layers[i] = new ActionQueue();
						}
						layers[i].Add(entry);
					}
				}
			}
		}
		public void ClearProcessed() {
			list.RemoveRange(0, index);
			index = 0;
		}
	}
}
