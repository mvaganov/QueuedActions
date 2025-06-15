using System;
using System.Collections.Generic;

namespace QueuedActions {
  public class key_input {
    public static key_input key_input_instance;
    public List<char> keys = new List<char>();
    public Dictionary<char, List<Action<object>>> key_binding = new Dictionary<char, List<Action<object>>>();
    private List<List<Action<object>>> to_execute_this_frame = new List<List<Action<object>>>();
    public key_input() {
      if (key_input_instance == null) {
        key_input_instance = this;
      }
    }
    public int count() {
      return keys.Count;
    }
    public char get_char(/*this,*/int index) {
      return this.keys[index];
    }
    public key_input get_instance() {
      if (key_input_instance != null) {
        return key_input_instance;
      }
      key_input_instance = new key_input();
      return key_input_instance;
    }
    public void update(/*this*/) {
      this.keys.Clear();
      while (Console.KeyAvailable) {
        ConsoleKeyInfo key = Console.ReadKey();
        this.keys.Add(key.KeyChar);
        if (this.key_binding.TryGetValue(key.KeyChar, out List<Action<object>> actions)) {
          this.to_execute_this_frame.Add(actions);
        }
      }
      this.to_execute_this_frame.ForEach(actions => actions.ForEach(a => a.Invoke(this)));
      this.to_execute_this_frame.Clear();
    }
    public bool has_key(/*this,*/char keyChar) {
      return get_key_index(keyChar) != -1;
    }
    public int get_key_index(/*this,*/char keyChar) {
      for (int i = 0; i < this.keys.Count; ++i) {
        if (this.keys[i] == keyChar) {
          return i;
        }
      }
      return -1;
    }
    public List<Action<object>> bind_key(/*this,*/char key, Action<object> action) {
      if (!this.key_binding.TryGetValue(key, out List<Action<object>> actions)) {
        actions = new List<Action<object>>();
        this.key_binding[key] = actions;
      }
      actions.Add(action);
      return actions;
    }
    /// <param name="key"></param>
    /// <param name="action">if null, removes all actions bound to this key</param>
    /// <returns></returns>
    public int unbind(/*this,*/char key, Action<object> action) {
      int removedCount = 0;
      if (!this.key_binding.TryGetValue(key, out List<Action<object>> actions)) {
        return removedCount;
      }
      int i = actions.Count - 1;
      while(i >= 0){
        if (action != null && actions[i] != action) {
          i -= 1;
          continue;
        }
        actions.RemoveAt(i);
        removedCount += 1;
        i -= 1;
      }
      return removedCount;
    }
  }
}
