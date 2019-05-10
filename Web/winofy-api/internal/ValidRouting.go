package internal

import (
	"../results"
	"github.com/emicklei/go-restful"
	"log"
)

type ValidRouting struct {

}


func (valid *ValidRouting) Route(ws *restful.WebService) {
	ws.Route(ws.POST("internal/valid").To(valid.registerFunc))
}


func (valid *ValidRouting) getResult(isValid bool) *results.TokenValidationResult {
	res := new(results.TokenValidationResult)
	res.Valid = isValid

	return res
}

func (valid *ValidRouting) registerFunc(request *restful.Request, response *restful.Response) {

	values, err := getParameters(request, []string{ "token", "username" })

	//FIXME variable "name" can inject SQL

	if err != nil {
		writeJsonResponse(response, valid.getResult(false), 200)
		log.Println(err)
		return
	}

	token := values[0]
	username := values[1]

	q := "SELECT Creation FROM Tokens WHERE Username = '" + username + "' AND Token = '" + token + "'"
	rows, err := sqlConnection.Query(q)
	defer rows.Close()

	if rows.Next() {
		writeJsonResponse(response, valid.getResult(true), 200)
		return
	}

	writeJsonResponse(response, valid.getResult(false), 200)
}