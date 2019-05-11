package internal

import (
	"github.com/emicklei/go-restful"
	"../results"
)

type NotificationRouting struct {

}

func (notif *NotificationRouting) Route(ws *restful.WebService) {
	ws.Route(ws.PUT("internal/notification").To(notif.putFunc))
}

func (notif *NotificationRouting) getResult(success bool) *results.NotificationResult {
	res := new(results.NotificationResult)
	res.Success = success

	return res
}

func (not *NotificationRouting) putFunc(request *restful.Request, response *restful.Response) {
	if !isAuthorized(request) {
		writeJsonResponse(response, not.getResult(false), 401)
		return
	}

	username, err := getUsernameFromToken(request.HeaderParameter("token"))

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 400)
		return
	}

	values, err := getBodyParameters(request, []string{ "device_token", "type" })
	dev_token := values[0]
	dev_type := values[1]

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 400)
		return
	}

	rows, err := sqlConnection.Query("SELECT DeviceToken, NotificationType FROM Notifications WHERE Username = ?", *username)
	defer rows.Close()

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 500)
		return
	}

	var (
		r_token string
		r_type string
		update bool
	)

	for rows.Next() {
		rows.Scan(&r_token, &r_type)

		if r_token == dev_token && r_type == dev_type {
			writeJsonResponse(response, not.getResult(true), 200)
			return
		}

		if r_token == dev_token {
			update = true
			break
		}
	}

	if update {
		exec := "UPDATE Notifications SET Username = ?, NotificationType = ? WHERE DeviceToken = ?"
		_, err = sqlConnection.Exec(exec, *username, dev_type, dev_token)

		if err != nil {
			writeJsonResponse(response, not.getResult(false), 500)
			return
		}

		writeJsonResponse(response, not.getResult(true), 200)
		return
	}

	exec := "INSERT INTO Notifications (Username, DeviceToken, NotificationType) VALUES (?, ?, ?)"
	_, err = sqlConnection.Exec(exec, *username, dev_token, dev_type)

	if err != nil {
		writeJsonResponse(response, not.getResult(false), 500)
		return
	}

	writeJsonResponse(response, not.getResult(true), 200)
}
