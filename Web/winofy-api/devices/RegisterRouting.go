package devices

import (
	"../results"
	"github.com/emicklei/go-restful"
)

type RegisterRouting struct {
}

func (reg *RegisterRouting) Route(ws *restful.WebService) {
	ws.Route(ws.POST("devices/register").To(reg.registerFunc))
}

func getRegisterResult(success bool, message string) *results.DeviceRegisterResult {
	res := new(results.DeviceRegisterResult)
	res.Success = success
	res.Message = message

	return res
}

func (reg *RegisterRouting) registerFunc(request *restful.Request, response *restful.Response) {
	if !isAuthorized(request) {
		writeJsonResponse(response, getRegisterResult(false, results.DeviceRegisterUnauthorized), 401)
		return
	}

	username, err := getUsernameFromToken(request.HeaderParameter("token"))
	if err != nil {
		writeJsonResponse(response, getRegisterResult(false, results.DeviceRegisterUnknown), 400)
		return
	}

	values, err := getBodyParameters(request, []string{ "device_id", "name", "description" })

	if err != nil {
		writeJsonResponse(response, getRegisterResult(false, results.DeviceRegisterUnknown), 400)
		return
	}

	dev := new(results.Device)
	dev.Id = values[0]
	dev.Name = values[1]
	dev.Description = values[2]

	if len(dev.Id) == 0 || len(dev.Name) == 0 {
		writeJsonResponse(response, getRegisterResult(false, results.DeviceRegisterUnknown), 400)
		return
	}

	q := "SELECT DeviceId FROM Devices WHERE Username = ?"
	rows, err := sqlConnection.Query(q, *username)
	defer rows.Close()

	if err != nil {
		writeJsonResponse(response, getRegisterResult(false, results.DeviceRegisterUnknown), 500)
		return
	}

	var (
		id string
	)

	for rows.Next() {
		rows.Scan(&id)

		if id == dev.Id {
			writeJsonResponse(response, getRegisterResult(false, results.DeviceRegisterDeviceExists), 409)
			return
		}
	}

	exec := "INSERT INTO Devices (DeviceId, Username, Name, Description, Creation) VALUES(?, ?, ?, ?, NOW())"
	_, err = sqlConnection.Exec(exec, dev.Id, *username, dev.Name, dev.Description)

	if err != nil {
		writeJsonResponse(response, getRegisterResult(false, results.DeviceRegisterUnknown), 500)
		return
	}

	res := results.DeviceRegisterResult{
		Success: true,
		Message: results.DeviceRegisterOk,
	}

	writeJsonResponse(response, res, 200)
}