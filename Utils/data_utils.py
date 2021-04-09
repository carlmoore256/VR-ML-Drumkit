import json
import os
import glob
import numpy as np

def save_json(out_path, data, indent=3):
  with open(out_path, 'w') as outfile:
    json.dump(data, outfile, sort_keys=False, indent=indent)
  print(f'wrote json to {out_path}')

# format data capture as a dict
def format_dict(data, keys):
    data_out = {}
    controller_ids = set([k.split("_")[-1] for k in keys])
    for cid in controller_ids: # set the key class
        data_out[cid] = {}
    for d, k in zip(data, keys):
        ctrl_id = k.split("_")[-1]
        class_id = k[:-3]
        data_out[ctrl_id][class_id] = d.tolist()
    return data_out

# converts a capture to json
def convert_capture_json(folder=None, out_path=None):
    if folder is None:
        folder = get_most_recent_dir()
        data, keys = load_capture_data()
    else:
        data, keys = load_capture_data(folder)

    data_dict = format_dict(data, keys)
    if out_path is None:
        tail = os.path.split(folder)[-1]
        out_path = os.path.join(folder, f"capture_{tail}.json")
    save_json(out_path, data_dict)

def listdirs(folder):
    folder = os.path.abspath(folder)
    return [os.path.join(folder, d) for d in os.listdir(folder) if os.path.isdir(os.path.join(folder, d))]

def get_most_recent_dir(path="../CapturedMotion/"):
    dirs = listdirs(path)
    return max(listdirs("../CapturedMotion/"), key=os.path.getctime)

def load_capture_data(capture_folder=None):
    if capture_folder is None:
        # load latest capture
        file_path = get_most_recent_dir()
    else:
        file_path = f"../CapturedMotion/{capture_folder}"
    
    all_files = glob.glob(f"{file_path}/*.csv")
    data = []
    keys = []
    for f in all_files:
        with open(f) as csvfile:
            capture = []
            for row in csvfile:
                row = row.rstrip("\n")
                row = row.split(",")
                row = [float(i) for i in row]
                capture.append(np.asarray(row))
            capture = np.asarray(capture)
            data.append(capture)
            keys.append(os.path.splitext(os.path.basename(f))[0])
    return data, keys