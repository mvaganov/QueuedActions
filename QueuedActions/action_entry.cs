using System;

namespace QueuedActions {
  // style looks like python because this needs to be converted into python
  public class action_entry_state {
    public bool started, finished;
    public void reset(/*this*/) { this.started = this.finished = false; }
  }
  public class action_entry_conditional {
    public string name;
    public Func<action_entry_record, bool> check;
    public Action<action_entry_record> activate;
    public action_entry_conditional(/*this,*/string name, Func<action_entry_record, bool> check, Action<action_entry_record> action) {
      this.name = name; this.check = check; activate = action;
    }
    public action_entry_conditional(/*this,*/string name, Func<action_entry_record, bool> check) { this.name = name; this.check = check; }
  }
  public class action_entry {
    public string id;
    public Action what_to_do;
    public float wait_time;
    public action_entry_conditional[] conditionals;
    public byte blocking_layer_mask;
    public bool uses_layers(/*this*/) => this.blocking_layer_mask != 0;
    public bool is_layer(/*this,*/int layer) { return (this.blocking_layer_mask & (1 << layer)) != 0; }
    public bool set_layer(/*this,*/int layer, bool blocked) {
      if (layer >= 255 || layer < 0)
        return false;
      if (blocked) {
        this.blocking_layer_mask &= (byte)layer;
      } else {
        this.blocking_layer_mask &= (byte)~layer;
      }
      return true;
    }
    public void start(/*this,*/ref action_entry_state state) {
      if (this.what_to_do != null) {
        this.what_to_do.Invoke();
      }
      state.started = true;
    }
    public bool is_finished_check(/*this,*/action_entry_record actionRecord) {
      if (actionRecord.state.finished)
        return true;
      if (this.conditionals == null) {
        actionRecord.state.finished = true; 
        return true;
      }
      bool finishByDefault = true;
      for (int i = 0; i < this.conditionals.Length; ++i) {
        action_entry_conditional condition = this.conditionals[i];
        if (condition == null || (condition.check == null && condition.activate == null)) {
          continue;
        }
        finishByDefault = false;
        if (condition.check == null || condition.check.Invoke(actionRecord)) {
          if (condition.activate != null) {
            condition.activate.Invoke(actionRecord);
          }
          actionRecord.state.finished = true;
          return true;
        }
      }
      return actionRecord.state.finished = finishByDefault;
    }
    public action_entry_conditional get_state(/*this,*/string name) {
      if (this.conditionals == null) { return null; }
      for (int i = 0; i < this.conditionals.Length; i++) {
        if (this.conditionals[i].name == name) {
          return this.conditionals[i];
        }
      }
      return null;
    }
    //public action_entry_conditional create_condition(string name) {
    //	if (conditionals == null) { conditionals = new action_entry_conditional[1]; } else { Array.Resize(ref conditionals, conditionals.Length + 1); }
    //	return conditionals[conditionals.Length - 1] = new action_entry_conditional(name);
    //}
  }
  public class action_entry_record {
    public action_entry_state state = new action_entry_state();
    public action_entry entry;
    public action_entry_record(/*this,*/action_entry entry) { this.entry = entry; }
    public void reset(/*this*/) {
      this.state.reset();
    }
    public bool is_started(/*this*/) {
      return this.state.started;
    }
    public bool is_finished(/*this*/) {
      return this.state.finished || this.entry.is_finished_check(this);
    }
    public void start(/*this*/) {
      this.entry.start(ref state);
    }
    public float wait_time(/*this*/) {
      return this.entry.wait_time;
    }
    public bool uses_layers(/*this*/) {
      return this.entry.uses_layers();
    }
  }
}
