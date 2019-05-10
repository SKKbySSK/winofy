package internal

import (
	"encoding/json"
	"github.com/emicklei/go-restful"
	"github.com/jmoiron/sqlx"
	"log"
	"time"
)

var sqlConnection *sqlx.DB

func SetSqlConnection(connection *sqlx.DB) {
	sqlConnection = connection
}

func getParameters(request *restful.Request, names []string) ([]string, error) {
	var results []string

	for _, name := range names {
		value, err := request.BodyParameter(name)

		if err != nil {
			return nil, err
		}

		results = append(results, value)
	}

	return results, nil
}

func writeJsonResponse(response *restful.Response, object interface{}, statusCode int) error {
	data, err := json.Marshal(object)
	response.WriteHeader(statusCode)
	response.Write(data)

	return err
}

func isAuthorized(request *restful.Request) bool {

	//FIXME variable "token" can inject SQL
	token := request.HeaderParameter("token")

	q := "SELECT Creation FROM Tokens WHERE Token = '" + token + "'"
	rows, err := sqlConnection.Query(q)

	if err != nil {
		log.Println("Error occured while checking auhorizing status")
		log.Println(err)
		return false
	}

	defer rows.Close()

	var creation time.Time
	if rows.Next() {
		err = rows.Scan(&creation)

		if err != nil {
			log.Println("Error occured while checking auhorizing status")
			log.Println(err)
			return false
		}

		//Token expires in 72 hours
		return true
	}

	return false
}