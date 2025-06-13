using System.Collections.Generic;

namespace QueuedActions {
  public class action_queue {
    public List<action_entry_record> list = new List<action_entry_record>();
    public int index;
    public float wait_time;
    public action_entry_record current;
    public int count(/*this*/) => this.list.Count;
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
    }
    public override string ToString(/*this*/) {
      string s = "@" + (this.index);
      if (this.index >= this.list.Count) {
        return s;
      }
      s += " [";
      for (int i = this.index; i < this.list.Count; ++i) {
        if (i > this.index) {
          s += "], [";
        }
        s += (this.list[i].entry.id);
      }
      s += "]";
      return s;
    }
    public void append(/*this,*/action_entry entry) {
      this.list.Add(new action_entry_record(entry));
    }
    public void remove_at(/*this*/int index) {
      this.list.RemoveAt(this.index);
    }

    public void insert_next(/*this,*/action_entry entry) {
      this.list.Insert(this.index+1, new action_entry_record(entry));
    }
    public void update(/*this,*/float deltaTime) {
      this.ensure_current_process();
      if (!this.is_current_process_started()) {
        this.start_current_process();
      }
      if (this.is_current_process_finished_including_wait_time(deltaTime)) {
        this.start_next_process();
      }
    }
    private void ensure_current_process(/*this*/) {
      if (this.index >= this.list.Count) {
        this.current = null;
      } else if (this.current == null && this.index < this.list.Count) {
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
      if (this.index >= list.Count) {
        return false;
      }
      this.current = this.list[this.index];
      this.current.start();
      float waitTime = this.current.wait_time();
      if (waitTime > 0)
        this.wait_time = waitTime;
      else
        this.wait_time = 0;
      return true;
    }
    public void update_self_and_distribute(/*this,*/float deltaTime, List<action_queue> layers) {
      if (this.wait_time > 0) {
        this.update(deltaTime);
        return;
      }
      while (this.index < this.list.Count) {
        this.current = this.list[this.index];
        if (this.current.is_started() && this.current.is_finished()) {
          this.index += 1;
          continue;
        }
        if (this.current.uses_layers()) {
          this.add_entry_to_layers(this.current.entry, layers);
          this.index += 1;
        } else {
          if (!this.is_current_process_started()) {
            this.start_current_process();
            if (!this.is_current_process_finished_including_wait_time(0)) {
              return;
            }
            this.index += 1;
          } else if (this.is_current_process_finished_including_wait_time(deltaTime)) {
            this.start_next_process();
            if (!this.is_current_process_finished_including_wait_time(0)) {
              return;
            }
          } else {
            return;
          }
        }
      }
    }
    void add_entry_to_layers(/*this,*/action_entry entry, List<action_queue> layers) {
      List<int> layersToAddTo = new List<int>();
      int maxLayer = -1;
      for (int i = 0; i < 8; i++) {
        if ((entry.blocking_layer_mask & (1 << i)) != 0) {
          layersToAddTo.Add(i);
          maxLayer = i;
        }
      }
      while (layers.Count <= maxLayer) {
        layers.Add(null);
      }
      for (int i = 0; i < layers.Count; ++i) {
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
