"""Check account statistics and status changes in a running disposable local database.

Set DCD_TEST_ADMIN_EMAIL and DCD_TEST_ADMIN_PASSWORD for the test administrator,
then run python tests/reports.py http://localhost:5080.
"""
import csv
import json
import os
import re
import sys
from io import StringIO
from uuid import uuid4

from accounts import session


def run(origin):
    admin, admin_form = session(origin)
    _, path, _ = admin_form("/account/sign-in", dict(
        Email=os.environ["DCD_TEST_ADMIN_EMAIL"], Password=os.environ["DCD_TEST_ADMIN_PASSWORD"]))
    assert path.startswith("/admin")

    def metrics():
        status, _, data = admin("/admin/reports/export.csv")
        assert status == 200
        day = list(csv.DictReader(StringIO(data.lstrip("\ufeff"))))[-1]
        page = admin("/admin/reports")[2]
        chart = json.loads(re.search(r'data-analytics-data>(.*?)</script>', page).group(1))[-1]
        counts = tuple(int(day[key]) for key in ("Sign-ups", "Total users", "Inactive users"))
        assert counts == (chart["signups"], chart["total"], chart["inactive"])
        assert int(day["Active users"]) == counts[1] - counts[2]
        return counts

    signups, total, inactive = metrics()
    member, member_form = session(origin)
    name = "Report" + uuid4().hex
    email = name + "@example.test"
    password = uuid4().hex + "A!"
    credentials = dict(Email=email, Password=password)
    assert member_form("/account/sign-up", dict(**credentials, ConfirmPassword=password,
                       FirstName=name, LastName="Check"))[1] == "/account/sign-in"
    assert member_form("/account/sign-in", credentials)[1] == "/"
    users = admin("/admin/users")[2]
    rows = re.findall(r"<li>(.*?)</li>", users, re.S)
    administrator = next(row for row in rows if os.environ["DCD_TEST_ADMIN_EMAIL"].casefold() in row.casefold())
    administrator_url = re.search(r'href="(/admin/users/[^"]+)"', administrator).group(1)
    assert admin_form(administrator_url + "/status", {"active": "false"}, "/admin/users")[0] == 403
    assert admin_form("/admin/users/" + uuid4().hex + "/status", {"active": "false"}, "/admin/users")[0] == 404
    row = next(row for row in rows if email in row)
    status_url = re.search(r'action="(/admin/users/[^"]+/status)"', row).group(1)
    assert admin(status_url.rsplit("/", 1)[0])[0] == 200
    assert metrics() == (signups + 1, total + 1, inactive)
    assert admin(status_url, {"active": "false"})[0] == 400
    for _ in range(2):
        assert admin_form(status_url, {"active": "false"}, "/admin/users")[0] == 200
    assert metrics() == (signups + 1, total + 1, inactive + 1)
    assert member("/")[1] == "/account/sign-in"
    assert member_form("/account/sign-in", credentials)[1] == "/account/sign-in"
    assert admin_form(status_url, {"active": "true"}, "/admin/users")[0] == 200
    assert member_form("/account/sign-in", credentials)[1] == "/"
    assert metrics() == (signups + 1, total + 1, inactive)
    assert "User deactivated" in admin("/admin/activity")[2]
    assert admin("/admin/reports/export.csv?start=bad")[0] == 400
    print("Live report totals, CSV, account status changes and session revocation passed.")


if __name__ == "__main__":
    run(sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5080")
