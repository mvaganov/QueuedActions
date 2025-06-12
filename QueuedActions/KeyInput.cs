using System;
using System.Collections.Generic;

namespace QueuedActions {
	public class KeyInput {
		private static KeyInput instance;
		List<ConsoleKeyInfo> keys = new List<ConsoleKeyInfo>();
		public int Count => keys.Count;
		public ConsoleKeyInfo this[int index] => keys[index];
		public KeyInput Instance => instance != null ? instance : instance = new KeyInput();
		public Dictionary<char, List<Action>> keyBinding = new Dictionary<char, List<Action>>();
		private List<List<Action>> toExecuteThisFrame = new List<List<Action>>();
		public KeyInput() {
			if (instance == null) {
				instance = this;
			}
		}
		public void Update() {
			keys.Clear();
			while (Console.KeyAvailable) {
				ConsoleKeyInfo key = Console.ReadKey();
				keys.Add(key);
				if (keyBinding.TryGetValue(key.KeyChar, out List<Action> actions)) {
					toExecuteThisFrame.Add(actions);
				}
			}
			toExecuteThisFrame.ForEach(actions => actions.ForEach(a => a.Invoke()));
			toExecuteThisFrame.Clear();
		}
		public bool HasKey(char keyChar) => GetKeyIndex(keyChar) != -1;
		public int GetKeyIndex(char keyChar) {
			for (int i = 0; i < keys.Count; i++) {
				if (keys[i].KeyChar == keyChar) { return i; }
			}
			return -1;
		}
		public List<Action> BindKey(char key, Action action) {
			if (!keyBinding.TryGetValue(key, out List<Action> actions)) {
				actions = new List<Action>();
				keyBinding[key] = actions;
			}
			actions.Add(action);
			return actions;
		}
		/// <param name="key"></param>
		/// <param name="action">if null, removes all actions bound to this key</param>
		/// <returns></returns>
		public int Unbind(char key, Action action) {
			int removedCount = 0;
			if (!keyBinding.TryGetValue(key, out List<Action> actions)) {
				return removedCount;
			}
			for (int i = actions.Count - 1; i >= 0; --i) {
				if (action != null && actions[i] != action) {
					continue;
				}
				actions.RemoveAt(i);
				++removedCount;
			}
			return removedCount;
		}
	}
}
