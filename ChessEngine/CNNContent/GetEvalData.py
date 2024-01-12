from tensorflow.keras.models import load_model
import numpy as np
from flask import Flask,request , jsonify
import random
model = load_model("TestedLast.keras")
count = 0

app = Flask(__name__)

def fen_to_combined_input(fen):
    board , turn ,castling   = fen.split()[:3] 

    board_matrix = np.zeros((8,8,12),dtype=np.int8)
    piece_map = {'P': 0, 'N': 1, 'B': 2, 'R': 3, 'Q': 4, 'K': 5,
               'p': 6, 'n': 7, 'b': 8, 'r': 9, 'q': 10, 'k': 11}
    
    for i, row in enumerate(board.split("/")):
        for j,char in enumerate(row):
            if char in piece_map:
                board_matrix[i,j, piece_map[char]] =1
            else:
                board_matrix[i,j,:] =12    

    turn_vector = np.zeros(2, dtype=np.int8)
    turn_vector[0 if turn == "w" else 1] = 1

  # Castling options representation (4-bit binary)
    castling_bits = 0
    for char in castling:
        if char in "KQkq":
            castling_bits |= 1 << (ord(char) - ord("K"))

  # Combine representations
    combined_input = np.concatenate((board_matrix.flatten(), turn_vector, np.array([castling_bits])), axis=0)

    return combined_input 


def GetEval(fen_string , weakModel):
    fen_string = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNR w KQkq - 0 1"

    if(weakModel == True):
       model =  load_model("TestedLast.keras")
    else:
       model = load_model("v1bad.keras")   


    # Convert FEN string to the model input format
    input_tensor = fen_to_combined_input(fen_string)[:768].reshape(8, 8, 12)

    # Reshape the input tensor to match the model's input shape
    input_tensor = np.expand_dims(input_tensor, axis=0)

    # Make a prediction using the trained model
    predicted_evaluation = model.predict(input_tensor)
    print(predicted_evaluation[0])
    return float( predicted_evaluation[0][0])


@app.route('/api/GetEval' , methods = ['POST'])
def RunGetEval():
    global count
    count = count +1 
    data = request.get_json()
    if not data:
        return jsonify({'message': f'request body is invalid '}), 666
    

    fen_string = data["fen"]
    weak_model = data["model"]
   # print("fen is ", fen_string)
  #  print("model is ", weak_model)
    count+=1

    result=  GetEval(fen_string ,weak_model)
    print("Count of times called is ", count)
    result = result *random.uniform(0.1, 22.9)
    print("Evaluation is -> ",result)
    return jsonify({'result': result})





if __name__ == "__main__":
    app.run( port=8000 ,debug= True , host="0.0.0.0")








