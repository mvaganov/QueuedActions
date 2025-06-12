using System;
using System.Collections.Generic;

namespace QueuedActions {
	public class ActionManager {
		/// <summary>
		/// processes to queue up
		/// </summary>
		public ActionQueue queue = new ActionQueue();
		/// <summary>
		/// parallel tasks to accomplish
		/// </summary>
		public List<ActionQueue> activeLayers = new List<ActionQueue>();
		public float WaitTime => queue.WaitTime;
		public void ResetAllActions() {
			queue.ResetAllActions();
			activeLayers.Clear();
		}
		public void ResetCurrentMainAction() {
			queue.Current.Reset();
			for (int i = 0; i < activeLayers.Count; i++) {
				int lastAction = activeLayers[i].Count - 1;
				if (lastAction >= 0 && activeLayers[i][lastAction] == queue.Current) {
					activeLayers[i].RemoveAt(lastAction);
				}
			}
		}
		public void Update(float deltaTime) {
			queue.UpdateSelfAndDistribute(deltaTime, activeLayers);
			for (int i = 0; i < activeLayers.Count; i++) {
				ActionQueue layer = activeLayers[i];
				if (layer == null) {
					continue;
				}
				layer.Update(deltaTime);
			}
		}
		public void Add(ActionEntry entry) {
			queue.Add(entry);
		}
		public void InsertNext(ActionEntry entry) {
			queue.InsertNext(entry);
		}
		public void PrintStuff() {
			Console.WriteLine($"(main) {queue}                                    ");
			for (int layer = 0; layer < activeLayers.Count; ++layer) {
				ActionQueue processLayer = activeLayers[layer];
				Console.WriteLine($"({layer}) {processLayer}                                       ");
			}
		}
	}
}
