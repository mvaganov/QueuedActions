using System;
using System.Collections;
using System.Collections.Generic;
//py from action_entry import *

namespace QueuedActions {
  public class action_queue {
    /*C#*/private static int len(IList list) {
    /*C#*/  return list.Count; // not needed in python
    /*C#*/}
    /*C#*/private static int len(string list) {
    /*C#*/  return list.Length; // not needed in python
    /*C#*/}
    /*C#*/private static string str(object o) {
    /*C#*/  return o.ToString(); // not needed in python
    /*C#*/}
    private static void _remove_at(IList list, int index) {
      /*C#*/list.RemoveAt(index);
      //py del list[index]
    }
    private static void _append(IList list, object item) {
      /*C#*/list.Add(item);
      //py list.append(item)
    }
    private static void _insert_at(IList list, int index, object item) {
      /*C#*/list.Insert(index, item);
      //py list.insert(index, item)
    }
    /*C#*/public List<action_entry_record> list = new List<action_entry_record>();
    /*C#*/public int index;
    /*C#*/public float wait_time;
    /*C#*/public action_entry_record current;
    //py def __init__(this):
    //py     this.list = []
    //py     this.index = 0
    //py     this.wait_time = 0
    //py     this.current = None
    public int count(/*this*/) {
      return len(this.list);
    }
    public action_entry_record get_record(/*this,*/int index) {
      return this.list[index];
    }
    public void set_record(/*this,*/int index, action_entry_record value) {
      this.list[index] = value;
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
      string s = "@" + str(this.index);
      //if (this.index >= this.count()) {
      //  return s;
      //}
      s += " ";
      int startingIndex = 0;
      for (int i = startingIndex; i < this.count(); i++) {
        if (i > startingIndex) {
          s += " ";
        }
        //if (i == this.index) {
        //  s += "[";
        //}
        if (i == this.index) {
          s += ">";
          s += (this.list[i].entry.id);
          //s += action_queue.first_letters(this.list[i].entry.id, 6);
        } else {
          s += this.AnnotateRecordTag(this.list[i]);
          s += (this.list[i].entry.id);
          //s += action_queue.first_letters(this.list[i].entry.id, 8);
        }
        //if (i == this.index) {
        //  s += "]";
        //}
      }
      s += "";
      return s;
    }
    private string AnnotateRecordTag(/*this,*/action_entry_record record) {
      int bitmask = record.state.finishConditionBitMask;
      if (bitmask == -1) {
        return ".";
      } else if (bitmask == 0) {
        return "-";
      } else if (bitmask == 1) {
        return "!";
      } else if (bitmask == 2) {
        return "X";
      }
      return "?";
    }
    public static string first_letters(string str, int letterCount) {
      if (len(str) > letterCount) {
        /*C#*/return str.Substring(0, letterCount);
        //py return str[0:letterCount]
      } else {
        return str;
      }
    }
    public void append(/*this,*/action_entry entry) {
      action_queue._append(this.list, new action_entry_record(entry));
    }
    public void remove_at(/*this,*/int index) {
      action_queue._remove_at(this.list, index);
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
    public void go_back_to_previous_process(/*this*/) {
      if (this.index >= 1) {
        if (this.current != null && this.current.entry.on_reset != null) {
          this.current.entry.on_reset(this.current);
        }
        this.index -= 1;
        this.ensure_current_process();
      }
    }
    private void ensure_current_process(/*this*/) {
      if (this.index >= this.count()) {
        this.current = null;
      } else if (this.index < this.count()) {
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
          action_queue._append(layersToAddTo, i);
          maxLayer = i;
        }
      }
      while (len(layers) <= maxLayer) {
        action_queue._append(layers, null);
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
