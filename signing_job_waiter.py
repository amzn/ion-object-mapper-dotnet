import sys
import boto3

client = boto3.client('signer')
waiter = client.get_waiter('successful_signing_job')

try:
    waiter.wait(jobId=sys.argv[1])
except Exception:
    sys.exit(1)
