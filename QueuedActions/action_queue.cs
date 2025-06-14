using System.Collections;
using System.Collections.Generic;
//py from action_entry import *

namespace QueuedActions {
  public class action_queue {
    private static int len(IList list) {
      return list.Count; // not needed in python
    }
    private static void _remove_at(IList list, int index) {
      list.RemoveAt(index); // del list[index]
    }
    private static void _append(IList list, object item) {
      list.Add(item); // list.append(item)
    }
    private static void _insert_at(IList list, int index, object item) {
      list.Insert(index, item); // list.insert(index, item)
    }
    public List<action_entry_record> list = new List<action_entry_record>();
    public int index;
    public float wait_time;
    public action_entry_record current;
    public int count(/*this*/) {
      return len(this.list);
    }
    public action_entry_record get_record(/*this,*/int index) {
      return this.list[this.index];
    }
    public void set_record(/*this,*/action_entry_record value) {
      this.list[this.index] = value;
    }
    public void reset_current_action(/*this*/) {
      if (this.current != null) {
        this.current.reset();
      }
    }
    public void reset_all_actions(/*this*/) {
      foreach (var a in this.list) {
        a.reset();
      }
      this.index = 0;
    }
    public override string ToString(/*this*/) {
      string s = "@" + (this.index);
      if (this.index >= this.count()) {
        return s;
      }
      s += " [";
      for (int i = this.index; i < this.count(); i++) {
        if (i > this.index) {
          s += "], [";
        }
        if (i <= this.index + 3) {
          s += (this.list[i].entry.id);
        }else {
          s += this.first_letters(this.list[i].entry.id, 2);
        }
      }
      s += "]";
      return s;
    }
    public string first_letters(string str, int letterCount) {
      if (str.Length >= letterCount) {
        return str.Substring(0, letterCount);
      } else {
        return str;
      }
    }
    public void append(/*this,*/action_entry entry) {
      action_queue._append(this.list, new action_entry_record(entry));
    }
    public void remove_at(/*this,*/int index) {
      action_queue._remove_at(this.list, this.index);
    }

    public void insert_next(/*this,*/action_entry entry) {
      action_queue._insert_at(this.list, this.index+1, new action_entry_record(entry));
    }
    public bool update(/*this,*/float deltaTime) {
      this.ensure_current_process();
      bool changeHappened = false;
      if (!this.is_current_process_started()) {
        this.start_current_process();
        changeHappened = true;
      }
      if (this.is_current_process_finished_including_wait_time(deltaTime)) {
        this.start_next_process();
        changeHappened = true;
      }
      return changeHappened;
    }
    private void ensure_current_process(/*this*/) {
      if (this.index >= this.count()) {
        this.current = null;
      } else if (this.current == null && this.index < this.count()) {
        this.current = this.list[this.index];
      }
    }
    public bool is_current_process_started(/*this*/) {
      return this.current == null || this.current.is_started();
    }
    public bool is_current_process_finished_including_wait_time(/*this,*/float deltaTime) {
      if (this.wait_time > 0) {
        this.wait_time -= deltaTime;
        if (this.wait_time > 0) {
          return false;
        }
        this.wait_time = 0;
      }
      return this.current == null || this.current.is_finished();
    }
    public bool start_next_process(/*this*/) {
      this.index += 1;
      return this.start_current_process();
    }
    public bool start_current_process(/*this*/) {
      if (this.index >= this.count()) {
        return false;
      }
      this.current = this.list[this.index];
      this.current.start();
      float waitTime = this.current.wait_time();
      if (waitTime > 0) {
        this.wait_time = waitTime;
      } else {
        this.wait_time = 0;
      }
      return true;
    }
    public bool update_self_and_distribute(/*this,*/float deltaTime, List<action_queue> layers) {
      if (this.wait_time > 0) {
        return this.update(deltaTime);
      }
      bool changeHappened = false;
      while (this.index < this.count()) {
        this.current = this.list[this.index];
        if (this.current.is_started() && this.current.is_finished()) {
          changeHappened = true;
          this.index += 1;
          continue;
        }
        if (this.current.uses_layers()) {
          changeHappened = true;
          this.add_entry_to_layers(this.current.entry, layers);
          this.index += 1;
        } else {
          if (!this.is_current_process_started()) {
            changeHappened = true;
            this.start_current_process();
            if (!this.is_current_process_finished_including_wait_time(0)) {
              break;
            }
            this.index += 1;
          } else if (this.is_current_process_finished_including_wait_time(deltaTime)) {
            changeHappened = true;
            this.start_next_process();
            if (!this.is_current_process_finished_including_wait_time(0)) {
              break;
            }
          } else {
            break;
          }
        }
      }
      return changeHappened;
    }
    private void add_entry_to_layers(/*this,*/action_entry entry, List<action_queue> layers) {
      List<int> layersToAddTo = new List<int>();
      int maxLayer = -1;
      for (int i = 0; i < 8; i++) {
        if ((entry.blocking_layer_mask & (1 << i)) != 0) {
          _append(layersToAddTo, i);
          maxLayer = i;
        }
      }
      while (len(layers) <= maxLayer) {
        _append(layers, null);
      }
      for (int i = 0; i < len(layers); i++) {
        if (entry.is_layer(i)) {
          if (layers[i] == null) {
            layers[i] = new action_queue();
          }
          layers[i].append(entry);
        }
      }
    }
    public void clear_processed(/*this*/) {
      this.list.RemoveRange(0, this.index);
      this.index = 0;
    }
  }
}
