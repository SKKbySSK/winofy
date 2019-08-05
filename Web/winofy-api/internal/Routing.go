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

func getBodyParameters(request *restful.Request, names []string) ([]string, error) {
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

func getUsernameFromToken(token string) (*string, error) {
	q := "SELECT Username FROM Tokens WHERE Token = ?"
	rows, err := sqlConnection.Query(q, token)
	defer rows.Close()

	if err != nil {
		return nil, err
	}

	for rows.Next() {
		var name string
		rows.Scan(&name)

		return &name, nil
	}

	return nil, nil
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

	q := "SELECT Creation FROM Tokens WHERE Token = ?"
	rows, err := sqlConnection.Query(q, token)

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